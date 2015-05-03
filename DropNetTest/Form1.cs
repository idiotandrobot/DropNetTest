using DropNet;
using DropNet.Models;
using DropNetTest.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DropNetTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public string AppKey = Settings.Default.AppKey;
        public string AppSecret = Settings.Default.AppSecret;

        public string UserToken { 
            get { return Settings.Default.UserToken; } 
            set { 
                Settings.Default.UserToken = value;
                Settings.Default.Save();
            } 
        }
        public string UserSecret { 
            get { return Settings.Default.UserSecret; } 
            set { 
                Settings.Default.UserSecret = value;
                Settings.Default.Save();
            } 
        }

        private DropNetClient _Client;
        public DropNetClient Client
        {
            get
            {
                if (_Client == null)
                {
                    _Client = new DropNetClient(AppKey, AppSecret);

                    if (IsAuthenticated)
                    {
                        _Client.UserLogin = new UserLogin
                        {
                            Token = UserToken,
                            Secret = UserSecret
                        };
                    }

                    _Client.UseSandbox = true;
                }

                return _Client;
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return !(string.IsNullOrEmpty(UserToken) ||
                    string.IsNullOrEmpty(UserSecret));
            }
        }

        public void Authenticate(Action<string> success, Action<Exception> failure)
        {
            Client.GetTokenAsync(userLogin =>
            {
                var url = Client.BuildAuthorizeUrl(userLogin);
                if (success != null) success(url);
            }, error =>
            {
                if (failure != null) failure(error);
            });
        }

        public void Authenticated(Action success, Action<Exception> failure)
        {
            Client.GetAccessTokenAsync((accessToken) =>
            {
                UserToken = accessToken.Token;
                UserSecret = accessToken.Secret;
                if (success != null) success();
            },
            (error) =>
            {
                if (failure != null) failure(error);
            });
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            Authenticate(
                   url => {
                       var proc = Process.Start(url);
                       proc.WaitForExit(); 
                        Authenticated(
                            () =>
                            {
                                MessageBox.Show("Authenticated");
                            },
                            exc => ShowException(exc));
                  
                   },
                   ex => ShowException(ex));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Client.AccountInfoAsync((a) =>
            {
                MessageBox.Show(a.display_name);
            },
            ex => ShowException(ex));
        }

        private void ShowException(Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }
    }
}
