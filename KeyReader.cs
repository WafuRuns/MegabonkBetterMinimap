using System;
using UnityEngine;

namespace MegabonkBetterMinimap {
    public static class KeyHelper {
        public static TimeSpan KeyCooldown { get; set; } = TimeSpan.FromMilliseconds(100);
        private static DateTime _lastKeyPressTime = DateTime.MinValue;

        public static bool IsKeyPressedInterval(KeyCode key) {
            if (!Input.GetKey(key))
                return false;

            DateTime now = DateTime.UtcNow;

            if (now - _lastKeyPressTime < KeyCooldown)
                return false;

            _lastKeyPressTime = now;
            return true;
        }
    }
}
