using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Principal;

namespace EPMJunkie.Core
{
    /// <summary>
    /// Impersonates a User Account
    /// </summary>
    /// <example>
    /// ImpersonateUser iu = new ImpersonateUser();
    /// iu.Impersonate("[domain]","[username]","[password]");
    /// // code you want to execute as impersonated user.....
    /// iu.Undo();
    /// </example>
    public class ImpersonateUser
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LogonUser(
        String lpszUsername,
        String lpszDomain,
        String lpszPassword,
        int dwLogonType,
        int dwLogonProvider,
        ref IntPtr phToken);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);
        private static IntPtr tokenHandle = new IntPtr(0);
        private static WindowsImpersonationContext impersonatedUser;
        // If you incorporate this code into a DLL, be sure to demand that it
        // runs with FullTrust.
        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public void Impersonate(string domainName, string userName, string password)
        {
            //try
            {
                // Use the unmanaged LogonUser function to get the user token for
                // the specified user, domain, and password.
                tokenHandle = IntPtr.Zero;
                // ---- Step - 1
                // Call LogonUser to obtain a handle to an access token.
                bool returnValue = LogonUser(
                userName,
                domainName,
                password,
                (int)LogonType.NewCredentials,
                (int)LogonProvider.Default,
                ref tokenHandle); // tokenHandle - new security token
                if (false == returnValue)
                {
                    int ret = Marshal.GetLastWin32Error();
                    throw new System.ComponentModel.Win32Exception(ret);
                }
                // ---- Step - 2
                WindowsIdentity newId = new WindowsIdentity(tokenHandle);
                // ---- Step - 3
                {
                    impersonatedUser = newId.Impersonate();
                }
            }
        }
        // Stops impersonation
        public void Undo()
        {
            impersonatedUser.Undo();
            // Free the tokens.
            if (tokenHandle != IntPtr.Zero)
            {
                CloseHandle(tokenHandle);
            }
        }
        public enum LogonType
        {
            //This logon type is intended for users who will be interactively using the computer, such as a user being logged on
            //by a terminal server, remote shell, or similar process.
            //This logon type has the additional expense of caching logon information for disconnected operations;
            //therefore, it is inappropriate for some client/server applications,
            //such as a mail server.
            Interactive = 2,

            //This logon type is intended for high performance servers to authenticate plaintext passwords.
            //The LogonUser function does not cache credentials for this logon type.
            Network = 3,

            //This logon type is intended for batch servers, where processes may be executing on behalf of a user without
            //their direct intervention. This type is also for higher performance servers that process many plaintext
            //authentication attempts at a time, such as mail or Web servers.
            //The LogonUser function does not cache credentials for this logon type.
            Batch = 4,

            //Indicates a service-type logon. The account provided must have the service privilege enabled.
            Service = 5,

            //This logon type is for GINA DLLs that log on users who will be interactively using the computer.
            //This logon type can generate a unique audit record that shows when the workstation was unlocked.
            Unlock = 7,

            //This logon type preserves the name and password in the authentication package, which allows the server to make
            //connections to other network servers while impersonating the client. A server can accept plaintext credentials
            //from a client, call LogonUser, verify that the user can access the system across the network, and still
            //communicate with other servers.
            //NOTE: Windows NT:  This value is not supported.
            NetworkCleartext = 8,

            //This logon type allows the caller to clone its current token and specify new credentials for outbound connections.
            //The new logon session has the same local identifier but uses different credentials for other network connections.
            //NOTE: This logon type is supported only by the LOGON32_PROVIDER_WINNT50 logon provider.
            //NOTE: Windows NT:  This value is not supported.
            NewCredentials = 9
        }
        public enum LogonProvider
        {
            //Use the standard logon provider for the system.
            //The default security provider is negotiate, unless you pass NULL for the domain name and the user name
            //is not in UPN format. In this case, the default provider is NTLM.
            //NOTE: Windows 2000/NT:   The default security provider is NTLM.
            Default = 0,
            WinNT35 = 1,
            WinNT40 = 2,
            WinNT50 = 3
        }
    }
}
