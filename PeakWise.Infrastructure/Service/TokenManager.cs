using Microsoft.Extensions.Configuration;
using PeakWise.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeakWise.Infrastructure.Service
{
    public class TokenManager
    {
        private readonly List<ApiToken> _tokens;
        private readonly object _lock = new();

        public TokenManager(IConfiguration config)
        {
            _tokens = config.GetSection("Gemini:Tokens")
                .Get<List<string>>()!
                .Select(t => new ApiToken
                {
                    Value = t
                })
                .ToList();

            if (!_tokens.Any())
                throw new Exception("No tokens configured.");
        }

        private ApiToken GetToken(HashSet<string> triedTokens)
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;

                var token = _tokens
                    .Where(t =>
                        !t.IsPermanentlyDisabled &&
                        (t.DisabledUntil == null || t.DisabledUntil <= now) &&
                        !triedTokens.Contains(t.Value))
                    .OrderBy(t => t.LastUsed)
                    .FirstOrDefault();

                if (token == null)
                    throw new Exception("No available tokens (all cooling down or failed).");

                token.LastUsed = now;
                return token;
            }
        }

        private void MarkFailed(ApiToken token, Exception ex)
        {
            lock (_lock)
            {
                token.FailCount++;

                if (ex.Message.Contains("403") || ex.Message.Contains("PERMISSION_DENIED"))
                {
                    token.IsPermanentlyDisabled = true;
                    return;
                }

                if (ex.Message.Contains("429"))
                {
                    token.DisabledUntil = DateTime.UtcNow.AddSeconds(30);
                    return;
                }

                var backoffSeconds = Math.Min(60, Math.Pow(2, token.FailCount));
                token.DisabledUntil = DateTime.UtcNow.AddSeconds(backoffSeconds);
            }
        }

        private void MarkSuccess(ApiToken token)
        {
            lock (_lock)
            {
                token.FailCount = 0;
                token.DisabledUntil = null;
            }
        }

        public async Task<T?> ExecuteWithRetry<T>(Func<string, Task<T>> action)
        {
            var triedTokens = new HashSet<string>();
            Exception? lastException = null;

            for (int i = 0; i < _tokens.Count; i++)
            {
                ApiToken token;

                try
                {
                    token = GetToken(triedTokens);
                    triedTokens.Add(token.Value);
                }
                catch (Exception ex)
                {
                    throw lastException ?? ex;
                }

                try
                {
                    var result = await action(token.Value);

                    MarkSuccess(token);
                    return result;
                }
                catch (Exception ex)
                {
                    MarkFailed(token, ex);
                    lastException = ex;

                    Console.WriteLine($"Token failed: {token.Value}");
                    Console.WriteLine($"Reason: {ex.Message}");

                    continue;
                }
            }

            throw lastException ?? new Exception("All tokens failed.");
        }
    }
}