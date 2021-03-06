﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using GameStack.Bindings;
#if __MOBILE__
using OpenTK.Graphics.ES20;
#else
using OpenTK.Graphics.OpenGL;
#endif
#if __ANDROID__
using PixelFormat = OpenTK.Graphics.ES20.All;
#endif

namespace GameStack.Content {
	public static class PngLoader {
		public static byte[] Decode (Stream stream, out Size size, out PixelFormat pxFormat) {
			var buf = new byte[stream.Length];
			stream.Read(buf, 0, buf.Length);
			stream.Close();
			return Decode(buf, out size, out pxFormat);
		}

		public static byte[] Decode (byte[] pngData, out Size size, out PixelFormat pxFormat) {
			IntPtr p;
			int w, h;
			var error = LibLodePng.Decode32(out p, out w, out h, pngData, (IntPtr)pngData.Length);
			if (error != 0) {
				throw new ContentException("Failed to load png");
				//throw new ContentException ("Error decoding PNG: " + LibLodePng.ErrorText (error));
			}
			var buf = new byte[w * h * 4];
			Marshal.Copy(p, buf, 0, buf.Length);
			LibLodePng.Free(p);
			size = new Size(w, h);
			pxFormat = PixelFormat.Rgba;
			return buf;
		}

		public static byte[] Encode (Stream stream, Size size) {
			var buf = new byte[stream.Length];
			stream.Read(buf, 0, buf.Length);
			stream.Close();
			return Encode(buf, size);
		}

		public static byte[] Encode (byte[] imgData, Size size) {
			IntPtr p;
			IntPtr ps;
			var error = LibLodePng.Encode32(out p, out ps, imgData, size.Width, size.Height);
			if (error != 0)
				throw new ContentException("Failed to save png");

			var buf = new byte[(long)ps];
			Marshal.Copy(p, buf, 0, buf.Length);
			LibLodePng.Free(p);
			return buf;
		}
	}
}
