using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DemoSilkX11.NativeAPI;
using static DemoSilkX11.NativeAPI.Win32Native;

namespace DemoSilkX11.Engine.Graphics.Windows
{
    public class RenderWindow : IDisposable
    {
        private IntPtr controlHandle;
        private Control control;
        public Control Control
        {
            get
            {
                return control;
            }
            set
            {
                if (control != value)
                {
                    if (control != null && !switchControl)
                    {
                        isControlAlive = false;
                        control.Disposed -= ControlDisposed;
                        controlHandle = IntPtr.Zero;
                    }

                    if (value != null && value.IsDisposed)
                    {
                        throw new InvalidOperationException("Control is already disposed");
                    }

                    control = value;
                    switchControl = true;
                }
            }
        }
        private bool isControlAlive;
        private bool switchControl;

        public bool UseApplicationDoEvents { get; set; }

        public Action? Render;
        public Action? Update;

        public static bool IsIdle
        {
            get
            {
                NativeMessage lpMsg;
                return PeekMessage(out lpMsg, IntPtr.Zero, 0, 0, 0) == 0;
            }
        }

        public RenderWindow()
        {
            Control = new Form();

        }
        public RenderWindow(Control control)
        {
            Control = control;
        }

        public bool NextFrame()
        {
            if (switchControl && control != null)
            {
                controlHandle = control.Handle;
                control.Disposed += ControlDisposed;
                isControlAlive = true;
                switchControl = false;
            }

            if (isControlAlive)
            {
                if (UseApplicationDoEvents)
                {
                    Application.DoEvents();
                }
                else if (controlHandle != IntPtr.Zero)
                {
                    NativeMessage lpMsg;
                    while (PeekMessage(out lpMsg, IntPtr.Zero, 0, 0, 0) != 0)
                    {
                        if (GetMessage(out lpMsg, IntPtr.Zero, 0, 0) == -1)
                        {
                            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "An error happened in rendering loop while processing windows messages. Error: {0}", new object[1]
                            {
                                Marshal.GetLastWin32Error()
                            }));
                        }

                        if (lpMsg.msg == 130)
                        {
                            isControlAlive = false;
                        }

                        Message message = default;
                        message.HWnd = lpMsg.handle;
                        message.LParam = lpMsg.lParam;
                        message.Msg = (int)lpMsg.msg;
                        message.WParam = lpMsg.wParam;
                        Message message2 = message;
                        if (!Application.FilterMessage(ref message2))
                        {
                            TranslateMessage(ref lpMsg);
                            DispatchMessage(ref lpMsg);
                        }
                    }
                }
            }

            if (!isControlAlive)
            {
                return switchControl;
            }

            return true;
        }

        public void Run()
        {
            control.Show();

            while (NextFrame())
            {
                Update?.Invoke();
                Render?.Invoke();
            }
        }

        private void ControlDisposed(object sender, EventArgs e)
        {
            isControlAlive = false;
        }
        public void Dispose()
        {
            Control = null;
        }
    }
}
