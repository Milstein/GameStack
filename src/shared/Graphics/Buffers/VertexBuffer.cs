#pragma warning disable 0618

using System;
using OpenTK;
using OpenTK.Graphics;

#if __DESKTOP__
using OpenTK.Graphics.OpenGL;
using BufferUsage = OpenTK.Graphics.OpenGL.BufferUsageHint;
#else
using OpenTK.Graphics.ES20;
#endif
#if __ANDROID__
using VertexAttribPointerType = OpenTK.Graphics.ES20.All;
using BufferTarget = OpenTK.Graphics.ES20.All;
using BufferUsage = OpenTK.Graphics.ES20.All;
#endif

namespace GameStack.Graphics {
	public class VertexBuffer : ScopedObject {
		VertexFormat _format;
		float[] _data;
		int _handle;
		#if __DESKTOP__
		int _vao;
		#endif

		public VertexBuffer (VertexFormat format, float[] vertices = null) {
			_format = format;
			_data = vertices;

			var buf = new int[1];
			GL.GenBuffers(1, buf);
			_handle = buf[0];
			#if __DESKTOP__
			_vao = GL.GenVertexArray();
			GL.BindVertexArray(_vao);
			#endif

			if (_data != null)
				this.Commit();
		}

		public VertexFormat Format { get { return _format; } }

		public float[] Data { get { return _data; } set { _data = value; } }

		public void Commit () {
			if (_data == null)
				_data = new float[0];

			GL.BindBuffer(BufferTarget.ArrayBuffer, _handle);
			GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(sizeof(float) * _data.Length), _data, BufferUsage.StaticDraw);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		}

		protected override void OnBegin () {
			var mat = ScopedObject.Find<Material>();
			if (mat == null)
				throw new InvalidOperationException("There is no active material.");
			if (ScopedObject.Find<VertexBuffer>() != null)
				throw new InvalidOperationException("there is already an active vertex buffer.");

			#if __DESKTOP__
			GL.BindVertexArray(_vao);
			#endif

			var stride = _format.Stride * sizeof(float);
			GL.BindBuffer(BufferTarget.ArrayBuffer, _handle);

			foreach (var el in _format.Elements) {
				var loc = mat.Shader.Attribute(el.Name);
				if (loc >= 0) {
					GL.EnableVertexAttribArray(loc);
					GL.VertexAttribPointer(loc, el.Size, VertexAttribPointerType.Float, false, stride, (IntPtr)(el.Offset * sizeof(float)));
				}
			}
		}

		protected override void OnEnd () {
			var mat = ScopedObject.Find<Material>();
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			if (mat != null) {
				foreach (var el in _format.Elements) {
					var loc = mat.Shader.Attribute(el.Name);
					if (loc >= 0)
						GL.DisableVertexAttribArray(loc);
				}
			}
		}

		public override void Dispose () {
			base.Dispose();

			if (_handle >= 0) {
				GL.DeleteBuffers(1, new int[] { _handle });
				_handle = -1;
			}
			#if __DESKTOP__
			if(_vao >= 0) {
				GL.DeleteVertexArray(_vao);
				_vao = -1;
			}
			#endif
		}
	}
}
