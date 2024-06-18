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
    public partial class SettingViewModel : ObservableObject
    {
        #region 修改
        /// <summary>
        /// 选择的用户
        /// </summary>
        public LoginPerson _loginPerson { get; set; } = new LoginPerson();

        private string _selectItem = "";
        /// <summary>
        /// 选择的项
        /// </summary>
        public string SelectItem
        {
            get { return _selectItem; }
            set
            {
                if (value!=null)
                {
                    if (LoginLoad.useNamesDictionary.Keys.Contains(value))
                    {
                        SetProperty(ref _selectItem, value);
                        LoginPerson PDic = LoginLoad.useNamesDictionary[_selectItem] as LoginPerson;
                        if (PDic != null)
                        {
                            _loginPerson.UserName = PDic.UserName;
                            _loginPerson.PassWord=PDic.PassWord;
                            _loginPerson.PrivileageLevel = PDic.PrivileageLevel;
                        }
                       
                    }
                }
               
            }
        }

        private List<string> _itemList = new List<string>();
        /// <summary>
        /// 用户名列表
        /// </summary>
        public List<string> ItemList
        {
            get
            {
                _itemList.Clear();
                foreach (string key in LoginLoad.useNamesDictionary.Keys)
                {
                    if (!string.IsNullOrEmpty(key) && key != "管理员")
                    {
                        _itemList.Add(key);
                    }
                }                
                return _itemList;
            }
            set
            {
               
               SetProperty(ref _itemList, value);
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


       

        private List<PRIVILEGE> _privileageList = new List<PRIVILEGE>();
        /// <summary>
        /// 用户名列表
        /// </summary>
        public List<PRIVILEGE> PrivileageList
        {
            get
            {
                _privileageList.Clear();
                Array eumnValue = Enum.GetValues(typeof(PRIVILEGE));
                foreach (PRIVILEGE val in eumnValue)
                {
                    if (val.ToString()!="管理员")
                    {
                        _privileageList.Add(val);
                    }
                    
                }
                return _privileageList;

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
                    ErrorMsg = "用户名不能为空,请输入用户名";
                    return;
                }
                if (string.IsNullOrWhiteSpace(AddPassword))
                {
                    ErrorMsg = "密码不能为空,请正确输入密码";
                    return;
                }

                if (LoginLoad.useNamesDictionary.Keys.Contains(AddUserName))
                {
                    ErrorMsg = "当前用户名已存在";
                    return;
                }
                if (string.IsNullOrEmpty(AddPrivlege.ToString()))
                {
                    ErrorMsg = "请选择用户名拥有的权限";
                    return;
                }
                LoginPerson p = new LoginPerson();
                p.UserName = AddUserName;
                p.PassWord = AddPassword;
                p.PrivileageLevel = AddPrivlege;

                LoginLoad.useNamesDictionary.Add(p.UserName, p);
                _itemList.Clear();
                List<string> tempList = new List<string>();
                foreach (string key in LoginLoad.useNamesDictionary.Keys)
                {
                    tempList.Add(key);
                }
                ItemList = tempList;
                LoginLoad.SaveUsers();
                ErrorMsg = "添加用户:" + p.UserName+"成功";
            }
            catch (Exception ex)
            {
                ErrorMsg = "修改出错:" + ex.Message;
            }



        }
        [RelayCommand]
        private void DeleteButton(object obj)
        {
            try
            {
                if (string.IsNullOrEmpty(_loginPerson.UserName))
                {
                    ErrorMsg = "请选择用户";
                    return;
                }
                if (!LoginLoad.useNamesDictionary.Keys.Contains(_loginPerson.UserName))
                {
                    ErrorMsg = "当前用户名不存在";
                    return;
                }
                if (_loginPerson.UserName == "管理员")
                {
                    ErrorMsg = "管理员不可删除";
                    return;
                }
                LoginLoad.useNamesDictionary.Remove(_loginPerson.UserName);
                _itemList.Clear();
                List<string> tempList = new List<string>();
                foreach (string key in LoginLoad.useNamesDictionary.Keys)
                {
                    tempList.Add(key);
                }
                ItemList = tempList;
                ErrorMsg = "删除用户:" + _loginPerson.UserName;
                _loginPerson.UserName = "";
                _loginPerson.PassWord = "";
                _loginPerson.PrivileageLevel = PRIVILEGE.无权限;
           
                LoginLoad.SaveUsers();

            }
            catch (Exception ex)
            {
                ErrorMsg = "修改出错:" + ex.Message;
            }
        }

        [RelayCommand]
        private void ChangeButton(object obj)
        {
            try
            {
                if (string.IsNullOrEmpty(_loginPerson.UserName))
                {
                    ErrorMsg = "请选择用户";
                    return;
                }

                if (string.IsNullOrEmpty(_loginPerson.PassWord))
                {
                    ErrorMsg = "请输入密码";
                    return;
                }

                if (!LoginLoad.useNamesDictionary.Keys.Contains(_loginPerson.UserName))
                {
                    ErrorMsg = "当前用户名不存在";
                    return;
                }
                if (string.IsNullOrEmpty(_loginPerson.PrivileageLevel.ToString()))
                {
                    ErrorMsg = "请选择用户名拥有的权限";
                    return;
                }

                var selectUser = LoginLoad.useNamesDictionary[_loginPerson.UserName];
                selectUser.PassWord = _loginPerson.PassWord;
                selectUser.PrivileageLevel = _loginPerson.PrivileageLevel;
                ErrorMsg = "修改成功";
                LoginLoad.SaveUsers();
            }
            catch (Exception ex)
            {
                ErrorMsg = "修改出错:" + ex.Message;
            }

        }
    }
}
