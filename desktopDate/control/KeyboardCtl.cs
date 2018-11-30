using csharpHelp.util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace desktopDate.control {
	public class KeyboardCtl {
		public bool isEnable = true;

		private int hKeyboardHook = -1;
		private User32.HookProc hKeyHookProc = null;
		private Dictionary<int, bool> mapKeyDown = new Dictionary<int, bool>();
		private Dictionary<int, int> mapVCtlKey = new Dictionary<int, int>();
		private uint keyData = 0;

		public Action<uint> checkKey = null;

		public void init() {
			mapVCtlKey.Clear();

			mapVCtlKey[162] = (byte)Key.LeftCtrl << 24;
			mapVCtlKey[163] = (byte)Key.RightCtrl << 24;
			mapVCtlKey[160] = (byte)Key.LeftShift << 16;
			mapVCtlKey[161] = (byte)Key.LeftShift << 16;
			mapVCtlKey[164] = (byte)Key.LeftAlt << 8;
			mapVCtlKey[165] = (byte)Key.LeftAlt << 8;

			hKeyHookProc = new User32.HookProc(keyHookProc);

			IntPtr hModel = Kernel32.GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);
			hKeyboardHook = User32.SetWindowsHookEx(HookType.WH_KEYBOARD_LL, hKeyHookProc, hModel, 0);
		}

		public void clear() {
			if (hKeyboardHook > 0) {
				User32.UnhookWindowsHookEx(hKeyboardHook);
				hKeyboardHook = -1;
			}
		}

		private int keyHookProc(int nCode, int wParam, IntPtr lParam) {
			// 侦听键盘事件
			do {
				if (nCode < 0) {
					break;
				}

				User32.KeyboardHookStruct MyKeyboardHookStruct = Marshal.PtrToStructure(lParam, typeof(User32.KeyboardHookStruct)) as User32.KeyboardHookStruct;
				if (MyKeyboardHookStruct == null) {
					break;
				}

				int vkey = MyKeyboardHookStruct.vkCode;
				bool isKeyDown = (wParam == User32.WM_KEYDOWN) || (wParam == User32.WM_SYSKEYDOWN);

				bool isRepeat = false;
				if (mapKeyDown.ContainsKey(vkey) && mapKeyDown[vkey] == isKeyDown) {
					isRepeat = true;
				}
				mapKeyDown[vkey] = isKeyDown;

				if (isRepeat) {
					break;
				}

				if (mapVCtlKey.ContainsKey(vkey)) {
					if (isKeyDown) {
						keyData = keyData | (uint)mapVCtlKey[vkey];
					} else {
						keyData = keyData & (uint)~mapVCtlKey[vkey];
					}
				} else {
					Key key = KeyInterop.KeyFromVirtualKey(MyKeyboardHookStruct.vkCode);
					if (isKeyDown) {
						keyData = keyData & 0xffffff00 | (byte)key;
					} else {
						keyData = keyData & 0xffffff00;
					}
				}

				if (!isEnable) {
					break;
				}

				//if (ShortcutBox.isGlobalEdit) {
				//	break;
				//}

				//if (isActivate || showSubWin) {
				//	break;
				//}

				if (!isKeyDown) {
					break;
				}

				checkKey?.Invoke(keyData);

			} while (false);

			//如果返回1，则结束消息，这个消息到此为止，不再传递。
			//如果返回0或调用CallNextHookEx函数则消息出了这个钩子继续往下传递，也就是传给消息真正的接受者
			return User32.CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
		}

	}
}
