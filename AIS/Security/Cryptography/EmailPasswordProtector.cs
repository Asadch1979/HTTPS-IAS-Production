using System;
using System.Security.Cryptography;
using System.Text;

namespace AIS.Security.Cryptography
    {
    public class EmailPasswordProtector
        {
        private const int NonceSize = 12;
        private const int TagSize = 16;
        private readonly byte[] _key;

        public EmailPasswordProtector()
            {
            var encodedKey = Environment.GetEnvironmentVariable("Email__EncryptionKey");
            if (string.IsNullOrWhiteSpace(encodedKey))
                {
                _key = Array.Empty<byte>();
                return;
                }

            try
                {
                _key = Convert.FromBase64String(encodedKey);
                if (_key.Length != 32)
                    {
                    _key = Array.Empty<byte>();
                    }
                }
            catch (FormatException)
                {
                _key = Array.Empty<byte>();
                }
            }

        public bool IsConfigured => _key.Length == 32;

        public byte[] Encrypt(string plaintext)
            {
            if (!IsConfigured)
                {
                throw new InvalidOperationException("The email credential encryption key is not configured.");
                }

            if (string.IsNullOrEmpty(plaintext))
                {
                throw new ArgumentException("A password is required for encryption.", nameof(plaintext));
                }

            var nonce = RandomNumberGenerator.GetBytes(NonceSize);
            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            var ciphertext = new byte[plaintextBytes.Length];
            var tag = new byte[TagSize];
            using var aes = new AesGcm(_key, TagSize);
            aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

            var payload = new byte[1 + NonceSize + TagSize + ciphertext.Length];
            payload[0] = 1;
            Buffer.BlockCopy(nonce, 0, payload, 1, NonceSize);
            Buffer.BlockCopy(tag, 0, payload, 1 + NonceSize, TagSize);
            Buffer.BlockCopy(ciphertext, 0, payload, 1 + NonceSize + TagSize, ciphertext.Length);
            CryptographicOperations.ZeroMemory(plaintextBytes);
            return payload;
            }

        public string Decrypt(byte[] payload)
            {
            if (!IsConfigured || payload == null || payload.Length <= 1 + NonceSize + TagSize || payload[0] != 1)
                {
                return string.Empty;
                }

            try
                {
                var ciphertextLength = payload.Length - 1 - NonceSize - TagSize;
                var plaintext = new byte[ciphertextLength];
                using var aes = new AesGcm(_key, TagSize);
                aes.Decrypt(
                    payload.AsSpan(1, NonceSize),
                    payload.AsSpan(1 + NonceSize + TagSize, ciphertextLength),
                    payload.AsSpan(1 + NonceSize, TagSize),
                    plaintext);
                var value = Encoding.UTF8.GetString(plaintext);
                CryptographicOperations.ZeroMemory(plaintext);
                return value;
                }
            catch (CryptographicException)
                {
                return string.Empty;
                }
            }
        }
    }
