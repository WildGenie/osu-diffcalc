﻿namespace OsuDiffCalc.UserInterface.Controls {
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Drawing;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Windows.Forms.DataVisualization.Charting;

	[DesignerCategory("")]
	public class CustomChart : Chart {
		private bool _isResizing = false;
		private Size _originalSize;
		public CustomChart() : base() {
			_originalSize = Size;
			SizeChanged += OnInitialSize;
		}

		private void OnInitialSize(object sender, EventArgs e) {
			_originalSize = Size;
			SizeChanged -= OnInitialSize;
		}

		private const string _autoSizeDescription = 
			"Offset to apply to size when using auto-sizing (Dock = Fill, for example). " +
			"Use negative values for 'left' or 'up' from default auto position. " +
			"These will be auto-rescaled as the chart size changes to try to maintiain the designer-shown proportions.";
		private const int
			_autoSizeLeftOffsetDefault = -14,
			_autoSizeRightOffsetDefault = 2,
			_autoSizeTopOffsetDefault = -4,
			_autoSizeBottomOffsetDefault = 2;
		private int
			_autoSizeLeftOffset = _autoSizeLeftOffsetDefault,
			_autoSizeRightOffset = _autoSizeRightOffsetDefault,
			_autoSizeTopOffset = _autoSizeTopOffsetDefault,
			_autoSizeBottomOffset = _autoSizeBottomOffsetDefault;

		[Category("Appearance"), Description(_autoSizeDescription), Bindable(true)]
		[DefaultValue(_autoSizeLeftOffsetDefault)]
		public int DockAutoSizeLeftOffset {
			get => _autoSizeLeftOffset;
			set => Set(ref _autoSizeLeftOffset, value);
		}

		[Category("Appearance"), Description(_autoSizeDescription), Bindable(true)]
		[DefaultValue(_autoSizeRightOffsetDefault)]
		public int DockAutoSizeRightOffset {
			get => _autoSizeRightOffset;
			set => Set(ref _autoSizeRightOffset, value);
		}

		[Category("Appearance"), Description(_autoSizeDescription), Bindable(true)]
		[DefaultValue(_autoSizeTopOffsetDefault)]
		public int DockAutoSizeTopOffset {
			get => _autoSizeTopOffset;
			set => Set(ref _autoSizeTopOffset, value);
		}

		[Category("Appearance"), Description(_autoSizeDescription), Bindable(true)]
		[DefaultValue(_autoSizeBottomOffsetDefault)]
		public int DockAutoSizeBottomOffset {
			get => _autoSizeBottomOffset;
			set => Set(ref _autoSizeBottomOffset, value);
		}

		protected override void OnResize(EventArgs e) {
			if (_isResizing) return;
			if (Dock != DockStyle.None) {
				try {
					_isResizing = true;
					double xScale = (double)Width / _originalSize.Width;
					double yScale = (double)Height / _originalSize.Height;
					int left   = Left   + Round(xScale * _autoSizeLeftOffset);
					int top    = Top    + Round(yScale * _autoSizeTopOffset);
					int width  = Width  + Round(xScale * (_autoSizeRightOffset - _autoSizeLeftOffset));
					int height = Height + Round(yScale * (_autoSizeBottomOffset - _autoSizeTopOffset));
					SetBounds(left, top, width, height);
				}
				finally {
					_isResizing = false;
				}
			}
			base.OnResize(e);
		}

		protected override void OnSizeChanged(EventArgs e) {
			if (_isResizing) return;
			base.OnSizeChanged(e); // calls this.OnResize()
		}

		protected override void OnClientSizeChanged(EventArgs e) {
			if (_isResizing) return;
			base.OnClientSizeChanged(e); // called by GUI.OnResize()
		}

		private bool Set<T>(ref T field, T value) {
			if (EqualityComparer<T>.Default.Equals(field, value)) return false;
			field = value;
			Invalidate();
			return true;
		}

		private static int Round(double value) {
			return (int)Math.Round(value, MidpointRounding.ToEven);
		}
	}
}
