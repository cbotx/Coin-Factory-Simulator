using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoinFactorySim {
    public class HotkeyHandler : IDisposable {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private int _hotkeyId;
        private IntPtr _hWnd;
        private Form _form;

        public event EventHandler HotkeyPressed;

        public HotkeyHandler(Form form, Keys key, uint modifiers) {
            _form = form;
            _hWnd = form.Handle;
            _hotkeyId = GetHashCode(); // Unique ID for the hotkey

            RegisterHotKey(_hWnd, _hotkeyId, modifiers, (uint)key);

            // Add a message filter to process WM_HOTKEY messages
            Application.AddMessageFilter(new HotkeyMessageFilter(this));
        }

        private class HotkeyMessageFilter : IMessageFilter {
            private HotkeyHandler _handler;

            public HotkeyMessageFilter(HotkeyHandler handler) {
                _handler = handler;
            }

            public bool PreFilterMessage(ref Message m) {
                if (m.Msg == 0x0312 && (int)m.WParam == _handler._hotkeyId) // WM_HOTKEY message
                {
                    _handler.OnHotkeyPressed();
                    return true; // Message handled
                }
                return false;
            }
        }

        protected virtual void OnHotkeyPressed() {
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose() {
            UnregisterHotKey(_hWnd, _hotkeyId);
            Application.RemoveMessageFilter(new HotkeyMessageFilter(this)); // Remove the message filter
        }
    }
}
