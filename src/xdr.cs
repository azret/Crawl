namespace System.Net
{
    using System.IO;
    using System.Text;

    public static class Xdr
    {
        public static string Execute(Uri uri, string method, 
            string data, string type,
            out int status, out Exception e, ICredentials credentials = null)
        {
            e = null;            

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(uri.ToString());

                if (credentials != null)
                {
                    request.Credentials = credentials;
                }

                request.Method = method;

                var bytes = data != null ? Encoding.UTF8.GetBytes(data) : null;

                if (bytes != null && bytes.Length > 0)
                {
                    request.ContentLength = bytes.Length;

                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }

                    if (string.IsNullOrWhiteSpace(type))
                    {
                        request.ContentType = type;
                    }
                }

                var response = (HttpWebResponse)request.GetResponse();

                try
                {
                    status = (int)response.StatusCode;

                    using (var stream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream))
                        {

                            data = reader.ReadToEnd();

                            if (data == null)
                            {
                                return String.Empty;
                            }

                            return data;
                        }
                    }
                }
                finally
                {
                    response.Dispose();
                }

            }
            catch (Exception innerError)
            { 
                e = innerError; status = 500;

                if (e is WebException)
                {
                    try
                    {
                        var response = ((WebException)e).Response as HttpWebResponse;

                        if (response != null)
                        {
                            status = (int)response.StatusCode;
                        }

                        using (Stream stream = ((WebException)e).Response.GetResponseStream())
                        {
                            using (var reader = new StreamReader(stream))
                            {
                                string msg = reader.ReadToEnd();

                                if (!string.IsNullOrWhiteSpace(msg))
                                {
                                    e = new WebException(
                                        msg,
                                        e,
                                        ((WebException)e).Status,
                                        ((WebException)e).Response
                                    );
                                }
                            }
                        }
                    }
                    catch (Exception fatal)
                    {
                        if (fatal is OutOfMemoryException || fatal is StackOverflowException)
                        {
                            throw fatal;
                        }
                    }
                }
                
                return null;
            }
        }
    }
}
