using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Zhucai.LambdaParser
{
    public static class ExpressionParserManager
    {

        private Dictionary<CacheKey, Delegate> _dicCache = new Dictionary<CacheKey, Delegate>();
        /// <summary>
 /// Analyze the Lambda expression in the form of strings to return a strong type delegate.
        /// There is no parameter, it may be omitted () =>, but it is not recommended.
        /// </ summary>
        /// <typeparam name = "tdlegate"> Entrusted type </ typeParam>
        /// <param name = "code"> Lambda expression code. </ param>
        /// <param name = "cache"> Whether to cache </ param>
        /// <returns> </ returns>
        static public TDelegate ParseDelegate<TDelegate>(string code, bool cache)
        {
            CacheKey key = null;
            if (cache)
            {
           // take it from the cache
                Delegate dele;
                Type type = typeof(TDelegate);
                key = new CacheKey(type, code);
                if (_dicCache.TryGetValue(key, out dele))
                {
                    return (TDelegate)(object)dele;
                }
            }

          // Cache is not created
            ExpressionParser<TDelegate> parser = new ExpressionParser<TDelegate>(code);
            Expression<TDelegate> expressionDele = parser.ToExpression();
            TDelegate tDele = expressionDele.Compile();

            if (cache)
            {
                // 并加入缓存
                lock (_dicCache)
                {
                    // Since the LOCK is not strict, it may be added repeatedly. If you use the ADD method, you may have an exception.
                    _dicCache[key] = tDele as Delegate;
                }
            }
            return tDele;
        }
        /// <summary>
/// Analyze the Lambda expression in the form of strings to return a strong type delegate.
        /// There is no parameter, you can omit () =>
        /// <typeparam name = "tdlegate"> Entrusted type </ typeParam>
        /// <param name = "code"> Lambda expression code. </ param>        /// <returns></returns>
        static public TDelegate ParseDelegate<TDelegate>(string code)
        {
            return ParseDelegate<TDelegate>(code, false);
        }

        /// <summary>
        /// 用来缓存时的Key
        /// </summary>
        private class CacheKey
        {
            public Type TDelegateType { get; private set; }
            public string Code { get; set; }
            public CacheKey(Type type, string code)
            {
                this.TDelegateType = type;
                this.Code = code;
            }
            public override int GetHashCode()
            {
                return this.Code.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }

                CacheKey cacheKey2 = (CacheKey)obj;

                if (this.TDelegateType != cacheKey2.TDelegateType)
                {
                    return false;
                }

                if (this.Code != cacheKey2.Code)
                {
                    return false;
                }

                return true;
            }
        }
    }
}
