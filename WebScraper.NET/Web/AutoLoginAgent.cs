using System.Collections.Generic;

namespace WebScraper.NET.Web
{
    public class AutoLoginAgent : Agent
    {
        public bool LoginRequired { get; set; }

        public List<IWebAction> LoginActions { get; set; }

        public List<IWebAction> LogoutActions { get; set; }

        public ExtractWebAction<bool> LoginCheckAction { get; set; }

        public AutoLoginAgent()
        {
            LoginRequired = true;
        }

        public void DoLogin()
        {
            DoActions(LoginActions);
        }
        public void DoLogout()
        {
            DoActions(LogoutActions);
            Cleanup();
        }
        public bool IsLoggedIn()
        {
            if (null == LoginCheckAction)
            {
                //if the login check is not present then it is a free resource
                return true;
            }

            LoginCheckAction.DoAction(this);
            return LoginCheckAction.ExtractedData;
        }
        public bool IsLoggedOut()
        {
            return !IsLoggedIn();
        }
    }
}
