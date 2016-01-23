using System.IO.IsolatedStorage;

namespace Voz.Model
{
	public class AppSettings
	{
		public IsolatedStorageSettings setting;

		public const string keyJoinDate = "ShowJoinDate";
		public const string keyLocation = "ShowLocation";
		public const string keyPosts = "ShowPosts";
		public const string keyAccount = "Account";
		public const string keyPassword = "Password";
		public const string keyDevice = "Device";
		public const string keyEmo = "ShowEmo";
		public const string keyAva = "ShowAva";
		public const string keyDarkTheme = "DarkTheme";
		public const string keyAccentColor = "AccentColor";
		public const string keyIsRated = "IsRated";
		public const string keyOpenCount = "OpenCount";
		public const string keyCookie = "Cookie";

		public AppSettings ()
		{
			setting = IsolatedStorageSettings.ApplicationSettings;
		}

		public void Add ( string key , object value )
		{
			if ( !setting.Contains ( key ) )
			{
				setting.Add ( key , value );
				setting.Save ();
			}
		}

		public void Update ( string key , object value )
		{
			if ( setting.Contains ( key ) )
			{
				if ( setting[key] != value )
				{
					setting[key] = value;
					setting.Save ();
				}
			}
		}

		public void RemoveKey ( string key )
		{
			if ( setting.Contains ( key ) )
			{
				setting.Remove ( key );
			}
		}

		public bool ContainKey ( string key )
		{
			if ( setting.Contains ( key ) )
				return true;
			return false;
		}

		public T GetValueOrDefault<T> ( string Key , T defaultValue )
		{
			T value;
			if ( setting.Contains ( Key ) )
			{
				value = ( T ) setting[Key];
			}
			else
			{
				value = defaultValue;
			}
			return value;
		}
	}
}
