// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-03-16
// Comment		
// **********************************************************************************************/

using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Pim.CommonLib;

namespace AspNetPim.Util
{
    /// <summary>
    ///     Incremental builder; current implementation passes query strings having null values as "ParamName:isnull=true",
    ///     similar to MS RS report viewer.
    /// </summary>
    public class UriBuilder2
    {
        /// <summary>
        ///     Instantiate
        /// </summary>
        /// <param name="path">
        ///     Base uri up to '?' mark separating it from query string
        /// </param>
        /// <param name="queryStringItems">
        ///     Optional collection representing query string
        /// </param>
        public UriBuilder2(string path, NameValueCollection queryStringItems)
        {
            Check.DoRequireArgumentNotNull(path, "path");

            Path = path;

            if (queryStringItems == null)
            {
                QueryStringItems = new NameValueCollection();
            }
            else
            {
                QueryStringItems = new NameValueCollection(queryStringItems);
            }
        }

        /// <summary>
        ///     Instantiate with empty query string.
        /// </summary>
        public UriBuilder2(string path)
            : this (path, null)
        {
        }

        public string Path { get; private set; }

        public NameValueCollection QueryStringItems { get; private set; }

        /// <summary>
        ///     Override to add internal items dynamically.
        /// </summary>
        /// <returns></returns>
        public virtual NameValueCollection GetFinalQueryStringItems()
        {
            return QueryStringItems;
        }

        public bool ForceAbsolute { get; set; }

        public UriBuilder2 AddQueryString(string name, object value)
        {
            QueryStringItems.Add(name, value != null ? value.ToString() : null);

            return this;
        }

        public override string ToString()
        {
            return MakeUri(Path, GetFinalQueryStringItems(), ForceAbsolute);
        }

        public static string MakeUri(string path, NameValueCollection queryStringItems, bool forceAbsolute)
        {
            var queryString = string.Join(
                "&"
                , queryStringItems.AllKeys
                            .SelectMany(
                                key => queryStringItems.GetValues(key) != null
                                    ? queryStringItems.GetValues(key)
                                        .Select(value => string.Format("{0}{1}={2}"
                                                     , HttpUtility.UrlEncode(key)
                                                     , value == null ? ":isnull" : string.Empty
                                                     , HttpUtility.UrlEncode(value ?? bool.TrueString)))
                                    : new string[] { string.Format("{0}:isnull=true", HttpUtility.UrlEncode(key)) }

                      ));

            var result = string.Format("{0}?{1}", path, queryString);

            if (forceAbsolute)
            {
                result = VirtualPathUtility.ToAbsolute(result);
            }

            return result;
        }
    }
}
