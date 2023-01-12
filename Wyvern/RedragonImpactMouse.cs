using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using HidSharp;

namespace Wyvern
{
    public class RedragonImpactMouse : IDisposable
    {
        public const int VendorId = 0x04D9;
        public const int ProductId = 0xFC4D;

        public const byte MinAnimationSpeed = 0;
        public const byte MaxAnimationSpeed = 15;

        private readonly HidDevice _device;
        private readonly HidStream _deviceStream;

        public RedragonImpactMouse()
        {
            var devices = FilteredDeviceList
                .Local
                .GetHidDevices(VendorId, ProductId)
                .ToArray();

            _device = devices.First(d => d.DevicePath.Contains("mi_02&col02"));
            _deviceStream = _device.Open();
        }

        public void DisablePolling()
            => Send(0x02, 0xF5, 0x00);

        public void EnablePolling()
            => Send(0x02, 0xF5, 0x01);

        public void SetSolidColor(Color color)
            => SetAnimation(LightAnimation.Breathe, 0, BreathingMode.Still, color);

        public void SetAnimation(LightAnimation animation, byte animationSpeed, byte animationParameter, Color color)
        {
            InAppearanceConfigurationMode(() => Send(
                0x02, 0xF3, 0x49, 0x04,
                0x06, 0x00, 0x00, 0x00,
                color.R, color.G, color.B, (byte)animation,
                (byte)(15 - ClampSpeedValue(animationSpeed)),
                animationParameter
            ));
        }

        public void SetAnimation(LightAnimation animation, byte animationSpeed)
            => SetAnimation(animation, animationSpeed, (byte)0, Color.Black);

        public void SetAnimation<T>(LightAnimation animation, byte animationSpeed, T animationParameter, Color color)
            where T : Enum
            => SetAnimation(
                animation,
                animationSpeed,
                (byte)Convert.ChangeType(animationParameter, TypeCode.Byte),
                color
            );

        public void SetAnimation<T>(LightAnimation animation, byte animationSpeed, T animationParameter) where T : Enum
            => SetAnimation(
                animation,
                animationSpeed,
                animationParameter,
                Color.Black
            );

        public void SetBrightnessLevel(BrightnessLevel level)
        {
            InAppearanceConfigurationMode(() => Send(
                0x02, 0xF3, 0x4F, 0x04,
                0x01, 0x00, 0x00, 0x00,
                (byte)level
            ));
        }

        public void Dispose()
            => _deviceStream.Dispose();

        public void Send(params byte[] bytes)
        {
            var packet = new byte[16];

            for (var i = 0; i < bytes.Length; i++)
                packet[i] = bytes[i];

            try
            {
                _deviceStream.SetFeature(packet);
            }
            catch (Win32Exception we)
            {
                if ((uint)we.NativeErrorCode != (uint)0x80000045)
                    throw;
            }
        }

        private void InAppearanceConfigurationMode([NotNull] Action perform)
        {
            Send(0x02, 0xF3, 0x46, 0x04, 0x02);

            perform();

            Send(0x02, 0xF1, 0x02, 0x04);
        }

        private byte ClampSpeedValue(byte b)
            => Math.Clamp(b, MinAnimationSpeed, MaxAnimationSpeed);
    }
}