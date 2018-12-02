using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;

namespace WebApp.Helpers
{
    public class MSALSessionCache
    {
        private static ReaderWriterLockSlim _sessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly string _userId = string.Empty;
        private readonly string _cacheId = string.Empty;
        private HttpContext _httpContext = null;

        private TokenCache _cache = new TokenCache();

        public MSALSessionCache(string userId, HttpContext httpcontext)
        {
            // not object, we want the SUB
            _userId = userId;
            _cacheId = _userId + "_TokenCache";
            _httpContext = httpcontext;
            Load();
        }

        public TokenCache GetMsalCacheInstance()
        {
            _cache.SetBeforeAccess(BeforeAccessNotification);
            _cache.SetAfterAccess(AfterAccessNotification);
            Load();
            return _cache;
        }

        public void SaveUserStateValue(string state)
        {
            _sessionLock.EnterWriteLock();
            _httpContext.Session.SetString(_cacheId + "_state", state);
            _sessionLock.ExitWriteLock();
        }

        public string ReadUserStateValue()
        {
            string state = string.Empty;
            _sessionLock.EnterReadLock();
            state = (string)_httpContext.Session.GetString(_cacheId + "_state");
            _sessionLock.ExitReadLock();
            return state;
        }

        public void Load()
        {
            _sessionLock.EnterReadLock();
            _cache.Deserialize(_httpContext.Session.Get(_cacheId));
            _sessionLock.ExitReadLock();
        }

        public void Persist()
        {
            _sessionLock.EnterWriteLock();

            // Optimistically set HasStateChanged to false. We need to do it early to avoid losing changes made by a concurrent thread.
            _cache.HasStateChanged = false;

            // Reflect changes in the persistent store
            _httpContext.Session.Set(_cacheId, _cache.Serialize());
            _sessionLock.ExitWriteLock();
        }

        // Triggered right before MSAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Load();
        }

        // Triggered right after MSAL accessed the cache.
        private void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (_cache.HasStateChanged)
            {
                Persist();
            }
        }
    }
}