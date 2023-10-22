using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace FeedManager.Abstractions
{
    public static class EncodingHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string EncodeId(string input)
        {
            var lowerCaseInput = input.ToLowerInvariant();
            var bytes = Encoding.UTF8.GetBytes(lowerCaseInput);
            return Convert.ToBase64String(bytes);
        }
    }
}
