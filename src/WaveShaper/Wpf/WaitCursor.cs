﻿using System;
using System.Windows.Input;

namespace WaveShaper.Wpf
{
    public class WaitCursor : IDisposable
    {
        private readonly Cursor _previousCursor;

        public WaitCursor()
        {
            _previousCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;
        }

        public void Dispose()
        {
            Mouse.OverrideCursor = _previousCursor;
        }
    }
}
