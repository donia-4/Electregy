using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PeakWise.Domain.Common;
using System.Threading.Tasks;

namespace PeakWise.Infrastructure.Service
{
    public class TokenManager
    {
        private readonly List<ApiToken> _tokens;
        private int _index = -1;
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
        }

        public ApiToken GetToken()
        {
            lock (_lock)
            {
                for (int i = 0; i < _tokens.Count; i++)
                {
                    _index = (_index + 1) % _tokens.Count;

                    var token = _tokens[_index];

                    if (!token.IsActive)
                        continue;

                    token.LastUsed = DateTime.UtcNow;
                    return token;
                }

                throw new Exception("No active tokens available");
            }
        }

        public void MarkFailed(string tokenValue)
        {
            lock (_lock)
            {
                var token = _tokens.FirstOrDefault(x => x.Value == tokenValue);
                if (token == null) return;

                token.FailCount++;

                if (token.FailCount >= 3)
                {
                    token.IsActive = false;
                }
            }
        }

        public void MarkSuccess(string tokenValue)
        {
            var token = _tokens.FirstOrDefault(x => x.Value == tokenValue);
            if (token == null) return;

            token.FailCount = 0;
        }

        public async Task<T?> ExecuteWithRetry<T>(Func<string, Task<T>> action)
        {
            for (int i = 0; i < _tokens.Count; i++)
            {
                var token = GetToken();

                try
                {
                    var result = await action(token.Value);
                    MarkSuccess(token.Value);
                    return result;
                }
                catch
                {
                    MarkFailed(token.Value);
                }
            }

            return default;
        }
    }
}
