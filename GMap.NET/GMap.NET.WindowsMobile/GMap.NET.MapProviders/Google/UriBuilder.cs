using System;
using System.Globalization;
using System.Runtime;
using System.Text;
using System.Threading;
namespace System
{
	public class UriBuilder
	{
		private bool _changed = true;
		private string _fragment = string.Empty;
		private string _host = "localhost";
		private string _password = string.Empty;
		private string _path = "/";
		private int _port = -1;
		private string _query = string.Empty;
		private string _scheme = "http";
		private string _schemeDelimiter = Uri.SchemeDelimiter;
		private Uri _uri;
		private string _username = string.Empty;
		private string Extra
		{
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}
				if (value.Length <= 0)
				{
					this.Fragment = string.Empty;
					this.Query = string.Empty;
					return;
				}
				if (value[0] == '#')
				{
					this.Fragment = value.Substring(1);
					return;
				}
				if (value[0] == '?')
				{
					int num = value.IndexOf('#');
					if (num == -1)
					{
						num = value.Length;
					}
					else
					{
						this.Fragment = value.Substring(num + 1);
					}

					this.Query = value.Substring(1, num - 1);
					return;
				}
				throw new ArgumentException("value");
			}
		}

		public string Fragment
		{
			get
			{
				return this._fragment;
			}
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}
				if (value.Length > 0)
				{
					value = '#' + value;
				}
				this._fragment = value;
				this._changed = true;
			}
		}

		public string Host
		{
			get
			{
				return this._host;
			}
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}
				this._host = value;
				if (this._host.IndexOf(':') >= 0 && this._host[0] != '[')
				{
					this._host = "[" + this._host + "]";
				}
				this._changed = true;
			}
		}
		
		public string Password
		{
			get
			{
				return this._password;
			}
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}
				this._password = value;
				this._changed = true;
			}
		}
		
		public string Path
		{
			get
			{
				return this._path;
			}
			set
			{
				if (value == null || value.Length == 0)
				{
					value = "/";
				}
				this._path = Uri.EscapeUriString(this.ConvertSlashes(value));
				this._changed = true;
			}
		}

		public int Port
		{
			get
			{
				return this._port;
			}
			set
			{
				if (value < -1 || value > 65535)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				this._port = value;
				this._changed = true;
			}
		}

		public string Query
		{
			get
			{
				return this._query;
			}
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}
				if (value.Length > 0)
				{
					value = '?' + value;
				}
				this._query = value;
				this._changed = true;
			}
		}

		public string Scheme
		{
			get
			{
				return this._scheme;
			}
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}
				int num = value.IndexOf(':');
				if (num != -1)
				{
					value = value.Substring(0, num);
				}
				if (value.Length != 0)
				{
					if (!Uri.CheckSchemeName(value))
					{
						throw new ArgumentException("value");
					}
					value = value.ToLower(CultureInfo.InvariantCulture);
				}
				this._scheme = value;
				this._changed = true;
			}
		}
		
		public Uri Uri
		{
			get
			{
				if (this._changed)
				{
					this._uri = new Uri(this.ToString());
					this.SetFieldsFromUri(this._uri);
					this._changed = false;
				}
				return this._uri;
			}
		}
		
		public string UserName
		{
			get
			{
				return this._username;
			}
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}
				this._username = value;
				this._changed = true;
			}
		}
		
		public UriBuilder()
		{
		}
		
		public UriBuilder(string uri)
		{
			Uri uri2 = new Uri(uri, UriKind.RelativeOrAbsolute);
			if (uri2.IsAbsoluteUri)
			{
				this.Init(uri2);
				return;
			}
			uri = Uri.UriSchemeHttp + Uri.SchemeDelimiter + uri;
			this.Init(new Uri(uri));
		}
		
		public UriBuilder(Uri uri)
		{
			if (uri == null)
			{
				throw new ArgumentNullException("uri");
			}
			this.Init(uri);
		}
		
		public UriBuilder(string schemeName, string hostName)
		{
			this.Scheme = schemeName;
			this.Host = hostName;
		}
		
		public UriBuilder(string scheme, string host, int portNumber) : this(scheme, host)
		{
			this.Port = portNumber;
		}
		
		public UriBuilder(string scheme, string host, int port, string pathValue) : this(scheme, host, port)
		{
			this.Path = pathValue;
		}
		
		public UriBuilder(string scheme, string host, int port, string path, string extraValue) : this(scheme, host, port, path)
		{
			try
			{
				this.Extra = extraValue;
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
				throw new ArgumentException("extraValue");
			}
		}
		
		public override bool Equals(object rparam)
		{
			return rparam != null && this.Uri.Equals(rparam.ToString());
		}
		
		public override int GetHashCode()
		{
			return this.Uri.GetHashCode();
		}
		
		public override string ToString()
		{
			if (this._username.Length == 0 && this._password.Length > 0)
			{
				throw new UriFormatException("net_uri_BadUserPassword");
			}
			if (this._scheme.Length != 0)
			{
                //UriParser syntax = UriParser.GetSyntax(this.m_scheme);
                //if (syntax != null)
                //{
                //   this.m_schemeDelimiter = ((syntax.InFact(UriSyntaxFlags.MustHaveAuthority) || (this.m_host.Length != 0 && syntax.NotAny(UriSyntaxFlags.MailToLikeUri) && syntax.InFact(UriSyntaxFlags.OptionalAuthority))) ? Uri.SchemeDelimiter : ":");
                //}
                //else
				{
					this._schemeDelimiter = ((this._host.Length != 0) ? Uri.SchemeDelimiter : ":");
				}
			}
			string text = (this._scheme.Length != 0) ? (this._scheme + this._schemeDelimiter) : string.Empty;
			return string.Concat(new string[]
			{
				text,
				this._username,
				(this._password.Length > 0) ? (":" + this._password) : string.Empty,
				(this._username.Length > 0) ? "@" : string.Empty,
				this._host,
				(this._port != -1 && this._host.Length > 0) ? (":" + this._port) : string.Empty,
				(this._host.Length > 0 && this._path.Length != 0 && this._path[0] != '/') ? "/" : string.Empty,
				this._path,
				this._query,
				this._fragment
			});
		}
		private void Init(Uri uri)
		{
			this._fragment = uri.Fragment; 
			this._query = uri.Query;
			this._host = uri.Host;
			this._path = uri.AbsolutePath;
			this._port = uri.Port;
			this._scheme = uri.Scheme;
         this._schemeDelimiter = (/*uri.HasAuthority*/!string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Host) ? Uri.SchemeDelimiter : ":");
			string userInfo = uri.UserInfo;
			if (!string.IsNullOrEmpty(userInfo))
			{
				int num = userInfo.IndexOf(':');
				if (num != -1)
				{
					this._password = userInfo.Substring(num + 1);
					this._username = userInfo.Substring(0, num);
				}
				else
				{
					this._username = userInfo;
				}
			}
			this.SetFieldsFromUri(uri);
		}
		private string ConvertSlashes(string path)
		{
			StringBuilder stringBuilder = new StringBuilder(path.Length);
			for (int i = 0; i < path.Length; i++)
			{
				char c = path[i];
				if (c == '\\')
				{
					c = '/';
				}
				stringBuilder.Append(c);
			}
			return stringBuilder.ToString();
		}
		private void SetFieldsFromUri(Uri uri)
		{
			this._fragment = uri.Fragment;
			this._query = uri.Query;
			this._host = uri.Host;
			this._path = uri.AbsolutePath;
			this._port = uri.Port;
			this._scheme = uri.Scheme;
         this._schemeDelimiter = (/*uri.HasAuthority*/!string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Host) ? Uri.SchemeDelimiter : ":");
			string userInfo = uri.UserInfo;
			if (userInfo.Length > 0)
			{
				int num = userInfo.IndexOf(':');
				if (num != -1)
				{
					this._password = userInfo.Substring(num + 1);
					this._username = userInfo.Substring(0, num);
					return;
				}
				this._username = userInfo;
			}
		}
	}
}
