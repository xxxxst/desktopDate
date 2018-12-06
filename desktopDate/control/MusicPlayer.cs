using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace desktopDate.control {
	public class MusicPlayer {
		/// <summary>   
		/// 文件地址   
		/// </summary>   
		public string path = "";
		public IntPtr handle = IntPtr.Zero;

		FMOD.System fmodSystem = null;
		FMOD.Sound backgroundMusic = null;
		FMOD.Channel channelMusic = null;

		/// <summary>   
		/// 播放   
		/// </summary>   
		public void play() {
			//mciSendString("close all", "", 0, 0);
			//mciSendString("open \"" + FilePath + "\" alias media", "", 0, 0);
			//mciSendString("play media", "", 0, 0);

			try {
				//if(fmodSystem != null) {
				//	stop();
				//}

				FMOD.RESULT fmodResult;
				if(fmodSystem == null) {
					// 1.创建System实例
					fmodResult = FMOD.Factory.System_Create(out fmodSystem);
					if(fmodResult != FMOD.RESULT.OK) {
						Debug.WriteLine(FMOD.Error.String(fmodResult));
						return;
					}
					// 2.初始化System
					fmodResult = fmodSystem.init(32, FMOD.INITFLAGS.NORMAL, IntPtr.Zero);
					if(fmodResult != FMOD.RESULT.OK) {
						Debug.WriteLine(FMOD.Error.String(fmodResult));
						return;
					}
				}

				//FMOD.Sound soundEffect; // 音效
				//FMOD.Sound backgroundMusic; // 背景音乐
											// 3.创建声音资源

				// 使用createSound创建音效 创建音效要用MODE.LOOP_OFF关闭循环播放(默认循环开启) out 一个Sound实例
				// createSound用来创建比较短不占用空间的音效,例如游戏中的攻击音效，可以在多个通道同时播放
				//fmodResult = fmodSystem.createSound("test.wav", FMOD.MODE.LOOP_OFF, out soundEffect);
				//if(fmodResult != FMOD.RESULT.OK) {
				//	Debug.WriteLine(FMOD.Error.String(fmodResult));
				//	return;
				//}

				// 使用createStream创建背景音乐 提供文件名 创建BGM可以默认开启循环 out 一个Sound实例
				// createStream用来创建比较长的声音，需要不停加载数据 例如游戏中的背景音乐，stream只能在一个通道播放
				fmodResult = fmodSystem.createStream(path, FMOD.MODE.LOOP_NORMAL, out backgroundMusic);
				if(fmodResult != FMOD.RESULT.OK) {
					Debug.WriteLine(FMOD.Error.String(fmodResult));
					return;
				}

				//FMOD.Channel channelEffect; // 音效的播放通道
				// 播放音乐 提供Sound 通道组我们没有 开始时是否暂停 是 out 当前声音的播放通道
				fmodSystem.playSound(backgroundMusic, null, false, out channelMusic);
				// 暂停来设置音量 0.0 - 1.0
				channelMusic.setVolume(0.5f);
				// 恢复播放
				//channelMusic.setPaused(false);
				// 按下ESC退出播放
				//while(Console.ReadKey().Key != ConsoleKey.Escape) {
				//	// 按下空格播放音效
				//	if(Console.ReadKey().Key == ConsoleKey.Spacebar)
				//		fmodSystem.playSound(soundEffect, null, false, out channelEffect);
				//}
				// 最后释放所有资源 可以代替.close();
				//fmodSystem.release();
			} catch(Exception ex) {
				Debug.WriteLine("aa:" + ex.ToString());
			}
		}

		public bool isPlay() {
			if(channelMusic == null) {
				return false;
			}

			bool rst = false;
			channelMusic.isPlaying(out rst);

			return rst;
		}

		/// <summary>   
		/// 暂停   
		/// </summary>   
		public void pause() {
			if(channelMusic == null) {
				return;
			}
			try {
				channelMusic.setPaused(true);
				//mciSendString("pause media", "", 0, 0);
			} catch(Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
		}

		/// <summary>   
		/// 停止   
		/// </summary>   
		public void stop() {
			if(channelMusic == null) {
				return;
			}

			try {
				//fmodSystem.close();
				//fmodSystem.release();
				//fmodSystem = null;

				channelMusic.stop();
				channelMusic = null;

				backgroundMusic.release();
				backgroundMusic = null;
			} catch(Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
			//mciSendString("close media", "", 0, 0);
		}

		///// <summary>   
		///// API函数   
		///// </summary>   
		//[DllImport("winmm.dll", EntryPoint = "mciSendString", CharSet = CharSet.Auto)]
		//private static extern int mciSendString(
		//	string lpstrCommand,
		//	string lpstrReturnString,
		//	int uReturnLength,
		//	int hwndCallback);
	}
}
