
using Microsoft.Win32;

namespace Service
{
	public sealed class ApiKeys
	{
		public string UspsUsername
		{
			get { return m_uspsUsername; }
		}

		public string UspsPassword
		{
			get { return m_uspsPassword; }
		}

		public static ApiKeys Load()
        {
			using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Bradley Grainger\AsyncIoDemo"))
			{
				string uspsUsername = key.GetValue("UspsUsername") as string;
				string uspsPassword = key.GetValue("UspsPassword") as string;

				if (uspsUsername == null || uspsPassword == null)
					return null;

				return new ApiKeys(uspsUsername, uspsPassword);
			}
        }

		private ApiKeys(string uspsUsername, string uspsPassword)
		{
			m_uspsUsername = uspsUsername;
			m_uspsPassword = uspsPassword;
		}

		readonly string m_uspsUsername;
		readonly string m_uspsPassword;
	}
}
