using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;



namespace WH.Controls
{
    /// <summary>
    /// 20240704 TCG
    /// 用户管理视图模型
    /// </summary>
    public partial class CSettingViewModel : ObservableObject
    {
        #region 修改
        /// <summary>
        /// 选择的用户
        /// </summary>
        public CLoginPerson LoginPerson { get; set; } = new CLoginPerson();

        private string selectItem = "";
        /// <summary>
        /// 选择的项
        /// </summary>
        public string SelectItem
        {
            get { return selectItem; }
            set
            {
                if (value!=null)
                {
                    if (LoginLoad.useNamesDictionary.Keys.Contains(value))
                    {
                        SetProperty(ref selectItem, value);
                        CLoginPerson PDic = LoginLoad.useNamesDictionary[selectItem] as CLoginPerson;
                        if (PDic != null)
                        {
                            LoginPerson.UserName = PDic.UserName;
                            LoginPerson.PassWord=PDic.PassWord;
                            LoginPerson.PrivileageLevel = PDic.PrivileageLevel;
                        }
                       
                    }
                }
               
            }
        }

        private List<string> itemList = new List<string>();
        /// <summary>
        /// 用户名列表
        /// </summary>
        public List<string> ItemList
        {
            get
            {
                itemList.Clear();
                foreach (string key in LoginLoad.useNamesDictionary.Keys)
                {
                    if (!string.IsNullOrEmpty(key) && key != "管理员")
                    {
                        itemList.Add(key);
                    }
                }                
                return itemList;
            }
            set
            {
               
               SetProperty(ref itemList, value);
            }
        }

        private string errorMsg = "";
        /// <summary>
        /// 信息显示
        /// </summary>
        public string ErrorMsg
        {
            get { return errorMsg; }
            set
            {
               // errorMsg = value;
               
               SetProperty(ref errorMsg, string.Format("{0}> {1}\r", DateTime.Now.ToString("T"), value));
            }
        }

       
        #endregion

        #region 注册

        private List<PRIVILEGE> privileageList = new List<PRIVILEGE>();
        /// <summary>
        /// 用户名列表
        /// </summary>
        public List<PRIVILEGE> PrivileageList
        {
            get
            {
                privileageList.Clear();
                Array eumnValue = Enum.GetValues(typeof(PRIVILEGE));
                foreach (PRIVILEGE val in eumnValue)
                {
                    if (val.ToString()!="管理员")
                    {
                        privileageList.Add(val);
                    }
                    
                }
                return privileageList;

            }
        }
        [ObservableProperty]
        private string addUserName = "";


        [ObservableProperty]
        private string addPassword = "";

        [ObservableProperty]
        private PRIVILEGE addPrivlege;
       
        #endregion


       
        [RelayCommand]
        private void AddButton(object obj)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AddUserName))
                {
                    ErrorMsg = Properties.Resources.InvalidUsername;
                    return;
                }
                if (string.IsNullOrWhiteSpace(AddPassword))
                {
                    ErrorMsg = Properties.Resources.InvalidPassword;
                    return;
                }

                if (LoginLoad.useNamesDictionary.Keys.Contains(AddUserName))
                {
                    ErrorMsg = Properties.Resources.UserAlreadyExist;
                    return;
                }
                if (string.IsNullOrEmpty(AddPrivlege.ToString()))
                {
                    ErrorMsg = Properties.Resources.PleaseSelectRights;
                    return;
                }
                CLoginPerson p = new CLoginPerson();
                p.UserName = AddUserName;
                p.PassWord = AddPassword;
                p.PrivileageLevel = AddPrivlege;

                LoginLoad.useNamesDictionary.Add(p.UserName, p);
                itemList.Clear();
                List<string> tempList = new List<string>();
                foreach (string key in LoginLoad.useNamesDictionary.Keys)
                {
                    tempList.Add(key);
                }
                ItemList = tempList;
                LoginLoad.SaveUsers();
                ErrorMsg = Properties.Resources.RegisterUser + p.UserName+Properties.Resources.Succeed;
            }
            catch (Exception ex)
            {
                ErrorMsg = Properties.Resources.ModifyError + ex.Message;
            }



        }
        [RelayCommand]
        private void DeleteButton(object obj)
        {
            try
            {
                if (string.IsNullOrEmpty(LoginPerson.UserName))
                {
                    ErrorMsg = Properties.Resources.PleaseSelectUser;
                    return;
                }
                if (!LoginLoad.useNamesDictionary.Keys.Contains(LoginPerson.UserName))
                {
                    ErrorMsg = Properties.Resources.UserNotExist;
                    return;
                }
                if (LoginPerson.UserName == "管理员")
                {
                    ErrorMsg = Properties.Resources.ManagerCanNotbeRemoved;
                    return;
                }
                LoginLoad.useNamesDictionary.Remove(LoginPerson.UserName);
                itemList.Clear();
                List<string> tempList = new List<string>();
                foreach (string key in LoginLoad.useNamesDictionary.Keys)
                {
                    tempList.Add(key);
                }
                ItemList = tempList;
                ErrorMsg = Properties.Resources.Delete + LoginPerson.UserName;
                LoginPerson.UserName = "";
                LoginPerson.PassWord = "";
                LoginPerson.PrivileageLevel = PRIVILEGE.NOPERMISSION;
           
                LoginLoad.SaveUsers();

            }
            catch (Exception ex)
            {
                ErrorMsg = Properties.Resources.ModifyError + ex.Message;
            }
        }

        [RelayCommand]
        private void ChangeButton(object obj)
        {
            try
            {
                if (string.IsNullOrEmpty(LoginPerson.UserName))
                {
                    ErrorMsg = Properties.Resources.PleaseSelectUser;
                    return;
                }

                if (string.IsNullOrEmpty(LoginPerson.PassWord))
                {
                    ErrorMsg = Properties.Resources.PleaseInputPassword;
                    return;
                }

                if (!LoginLoad.useNamesDictionary.Keys.Contains(LoginPerson.UserName))
                {
                    ErrorMsg = Properties.Resources.UserNotExist;
                    return;
                }
                if (string.IsNullOrEmpty(LoginPerson.PrivileageLevel.ToString()))
                {
                    ErrorMsg = Properties.Resources.PleaseSelectRights;
                    return;
                }

                var selectUser = LoginLoad.useNamesDictionary[LoginPerson.UserName];
                selectUser.PassWord = LoginPerson.PassWord;
                selectUser.PrivileageLevel = LoginPerson.PrivileageLevel;
                ErrorMsg = Properties.Resources.Succeed;
                LoginLoad.SaveUsers();
            }
            catch (Exception ex)
            {
                ErrorMsg = Properties.Resources.ModifyError + ex.Message;
            }

        }
    }
}
