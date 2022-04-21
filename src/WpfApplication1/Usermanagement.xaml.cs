using AssetBuilder.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AssetBuilder
{
    /// <summary>
    /// Interaction logic for Usermanagement.xaml
    /// </summary>
    public partial class Usermanagement : ABWindow
    {
        static UM.Uman uman;
        public static Usermanagement window = null;

        private bool pageControlVisibilityEnabled { get; set; }
        private int companyID { get; set; }
        private string CompanySprojOid { get; set; }
        private int UserId { get; set; }
        private string UserSprojOid { get; set; }

        protected override void OnClosed(EventArgs e)
        {
            window = null;
        }

        public Usermanagement(string assetbuilderLogin)
        {
            pageControlVisibilityEnabled = false;
            InitializeComponent();

            uman = new UM.Uman(Window1.UserName);

            if(uman.userSecurity.UserSprojoid == "")
            {
                MessageBox.Show("Unable to validate user");
                return;
            }

            PopulateCompanies();
            PopulateCompanies_MoveTo();
            pageControlVisibilityEnabled = true;
            PageControlVisibility(true);
            SecurityDisplay();
            window = this;
        }


        #region Display Layer"
        public class Company
        {
            public string sprojoid { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }

            public List<Company> GetCompanies(int excludeCompanyID=0)
            {
                List<Company> ComplanyList = new List<Company>();

                UM.Uman.Company company = new UM.Uman.Company();
                List<UM.Uman.Company> companies = company.GetCompanies(uman.userSecurity);

                if (companies != null)
                {
                    foreach (UM.Uman.Company obj in companies)
                    {
                        if (excludeCompanyID != obj.id)
                        {
                            Company row = new Company
                            {
                                Id = obj.id,
                                Name = obj.name,
                                sprojoid = obj.sprojoid
                            };

                            ComplanyList.Add(row);
                        }
                    }
                }

                return ComplanyList;
            }

        }
        public class UserInfo
        {
            UserRole UR = new UserRole();
            public string sprojoid { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
            public string Status { get; set; }
            public string UserStatus(bool orphaned, bool active, int roleCount)
            {
                string userStatus = "Active";

                if(roleCount == 0)
                {
                    userStatus += " (Sync Required) ";
                }

                if (orphaned)
                {
                    userStatus = "Orphaned";
                    if (roleCount == 0)
                    {
                        userStatus += " (Sync Required) ";
                    }
                }
                else if (!active)
                {
                    userStatus = "Inactive";
                }

                return userStatus;
            }
            public int Roles { get; set; }
            public List<UserRole> userRoles { get; set; }

            public List<UserInfo> GetUsers2(string companySpoid, string userSpoid = "")
            {
                List<UserInfo> UserList = new List<UserInfo>();
                UM.Uman.UserInfo userInfo = new UM.Uman.UserInfo();
                var users = userInfo.GetUserInfos2(uman.userSecurity, companySpoid: companySpoid);

                foreach (var user in users["Users"])
                {
                    var info = new UserInfo
                    {
                        Id = user["PK_User_ID"],
                        Name = user["UName"],
                        Roles = user["Roles"].Count,
                        sprojoid = user["SPROJOID"],
                        Status = UserStatus(user["Orphaned"], user["Active"], user["Roles"].Count),
                    };
                    UserList.Add(info);
                    info.userRoles = users["DBRoles"]
                        .Select(f => new UserRole
                        {
                            Assigned = user["Roles"].Contains(f["PK_Role_ID"]),
                            Id = f["PK_Role_ID"],
                            Name = f["Role_Name"],
                            sprojoid = f["SPROJOID"]
                        }).ToList();
                }

                return UserList;
            }

            public List<UserInfo> GetUsers(string companySpoid, string userSpoid = "")
            {
                List<UserInfo> UserList = new List<UserInfo>();

                UM.Uman.UserInfo userInfo = new UM.Uman.UserInfo();
                List<UM.Uman.UserInfo> UserInfos = userInfo.GetUserInfos(uman.userSecurity, companySpoid: companySpoid);

                foreach (UM.Uman.UserInfo obj in UserInfos)
                {
                    int roleAssignedCount = 0;

                    if (userSpoid != "")
                    {
                        if (userSpoid == obj.sprojoid)
                        {
                            UserInfo row = new UserInfo
                            {
                                sprojoid = obj.sprojoid,
                                Id = obj.userID,
                                Name = obj.userName,
                                userRoles = SerializeUserRoles(obj.userRoles),
                                Status = UserStatus(obj.orphaned, obj.active, obj.userRoles.Count())
                            };
                            foreach (UserRole userRole in row.userRoles)
                            {
                                if (userRole.Assigned) { roleAssignedCount += 1; }
                            }
                            row.Roles = roleAssignedCount;
                            UserList.Add(row);
                            break;
                        }
                    }
                    else
                    {
                        UserInfo row = new UserInfo
                        {
                            sprojoid = obj.sprojoid,
                            Id = obj.userID,
                            Name = obj.userName,
                            userRoles = SerializeUserRoles(obj.userRoles),
                            Status = UserStatus(obj.orphaned, obj.active, obj.userRoles.Count())
                        };
                        foreach (UserRole userRole in row.userRoles)
                        {
                            if (userRole.Assigned) { roleAssignedCount += 1; }
                        }
                        row.Roles = roleAssignedCount;
                        UserList.Add(row);
                    }
                }

                return UserList;
            }
            public UserInfo GetUser(string userSpoid)
            {
                UM.Uman.UserInfo coreUserInfo = new UM.Uman.UserInfo();
                UM.Uman.UserInfo usr = coreUserInfo.GetUserInfo(uman.userSecurity, userSpoid);

                var usrRoles = 0;

                if(usr.userRoles != null)
                {
                    usrRoles = usr.userRoles.Count();
                }

                UserInfo userInfo = new UserInfo
                {
                    Id = usr.userID,
                    Name = usr.userName,
                    sprojoid = usr.sprojoid,
                    userRoles = SerializeUserRoles(usr.userRoles),
                    Status = UserStatus(usr.orphaned, usr.active, usrRoles)
                };

                foreach (UserRole userRole in userInfo.userRoles)
                {
                    if (userRole.Assigned)
                    {
                        userInfo.Roles += 1;
                    }
                }

                return userInfo;
            }
            private List<UserRole> SerializeUserRoles(List<UM.Uman.UserRole> uRoles)
            {
                List<UserRole> userRoles = new List<UserRole>();

                if (uRoles != null)
                {
                    foreach (UM.Uman.UserRole userRole in uRoles)
                    {
                        UserRole role = new UserRole
                        {
                            sprojoid = userRole.sprojoid,
                            Id = userRole.roleID,
                            Name = userRole.roleName,
                            Assigned = userRole.assignedToRoleInSproj
                        };

                        userRoles.Add(role);
                    }
                }

                return userRoles;
            }
            public UserInfo GetUserDetail(string userSpoid = "", string userName = "")
            {
                UserInfo userInfo = new UserInfo();

                if (userSpoid != "")
                {
                    userInfo = GetUser(userSpoid);
                }
                else
                {
                    userInfo = GetUser(userName);
                }

                return userInfo;
            }
            //public List<UserInfoSessionLoginHistory> GetUserInfoSessionLoginHistories(string userSpoid)
            //{
            //    UM.Uman.OverviewHistory overviewHistory = new UM.Uman.OverviewHistory(uman.userSecurity, userSpoid);
            //    List<UM.Uman.UserInfoSessionLoginHistory> History = overviewHistory.UserInfoSessionLoginHistories;

            //    List<UserInfoSessionLoginHistory> histories = new List<UserInfoSessionLoginHistory>();

            //    foreach(UM.Uman.UserInfoSessionLoginHistory userInfoSessionLoginHistory in History)
            //    {
            //        UserInfoSessionLoginHistory history = new UserInfoSessionLoginHistory
            //        {
            //            DomainLoginAccount = userInfoSessionLoginHistory.DomainLoginAccount,
            //            Current = userInfoSessionLoginHistory.Current,
            //            Previous = userInfoSessionLoginHistory.Previous,
            //            Former = userInfoSessionLoginHistory.Former
            //        };

            //        histories.Add(history);
            //    }

            //    return histories;

            //}
            //public List<UserRoleAssignment> GetUserRoleAssignment(string userSpoid)
            //{
            //    UM.Uman.OverviewHistory overviewHistory = new UM.Uman.OverviewHistory(uman.userSecurity, userSpoid);
            //    UM.Uman.UserRoleAssignment Assignment = overviewHistory.UserRoleAssignment;

            //    List<UserRoleAssignment> userRoleAssignments = new List<UserRoleAssignment>();

            //    UserRoleAssignment userRoleAssignment = new UserRoleAssignment
            //    {
            //        AlgoAdmin = Assignment.AlgoAdmin,
            //        AlgoBuilders = Assignment.AlgoBuilders,
            //        AlgoEditor = Assignment.AlgoEditor,
            //        AlgoReviewers = Assignment.AlgoReviewers,
            //        AlgoTranslator = Assignment.AlgoTranslator,
            //        Comments = Assignment.Comments,
            //    };

            //    userRoleAssignments.Add(userRoleAssignment);

            //    return userRoleAssignments;
            //}
            //public List<UserRoleAssignmentHistory> GetUserRoleAssignmentHistories(string userSpoid)
            //{
            //    UM.Uman.OverviewHistory overviewHistory = new UM.Uman.OverviewHistory(uman.userSecurity, userSpoid);
            //    List<UM.Uman.UserRoleAssignmentHistory> History = overviewHistory.UserRoleAssignmentHistories;

            //    List<UserRoleAssignmentHistory> histories = new List<UserRoleAssignmentHistory>();

            //    foreach (UM.Uman.UserRoleAssignmentHistory userRoleAssignmentHistory in History)
            //    {
            //        UserRoleAssignmentHistory history = new UserRoleAssignmentHistory
            //        {
            //            ModifiedBy = userRoleAssignmentHistory.ModifiedBy,
            //            Role = userRoleAssignmentHistory.Role,
            //            Add = userRoleAssignmentHistory.Add,
            //            Remove = userRoleAssignmentHistory.Remove
            //        };

            //        histories.Add(history);
            //    }

            //    return histories;
            //}
            //public List<UserAudit> GetUserAudits(string userSpoid)
            //{
            //    UM.Uman.OverviewHistory overviewHistory = new UM.Uman.OverviewHistory(uman.userSecurity, userSpoid);
            //    List<UM.Uman.UserAudit> History = overviewHistory.UserAudit;

            //    List<UserAudit> histories = new List<UserAudit>();

            //    foreach (UM.Uman.UserAudit audit in History)
            //    {
            //        UserAudit history = new UserAudit
            //        {
            //           Date = audit.Date,
            //           ProcessName = audit.ProcessName,
            //           Summary = audit.Summary,
            //           Detail = audit.Detail,
            //           LastModifiedBy = audit.LastModifiedBy
            //        };

            //        histories.Add(history);
            //    }

            //    return histories;
            //}
        }
        public class UserRole
        {
            public string sprojoid { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
            public bool Assigned { get; set; }
        }
        public class UserInfoSessionLoginHistory
        {
            public string DomainLoginAccount { get; set; }
            public DateTime Current { get; set; }
            public DateTime Previous { get; set; }
            public DateTime Former { get; set; }
        }
        public class UserRoleAssignment
        {
            public bool AlgoEditor { get; set; }
            public bool AlgoTranslator { get; set; }
            public bool Comments { get; set; }
            public bool AlgoAdmin { get; set; }
            public bool AlgoBuilders { get; set; }
            public bool AlgoReviewers { get; set; }
        }
        public class UserRoleAssignmentHistory
        {
            public string ModifiedBy { get; set; }
            public string Role { get; set; }
            public string Add { get; set; }
            public string Remove { get; set; }
        }
        public class UserAudit
        {
            public string ProcessName { get; set; }
            public DateTime Date { get; set; }
            public string Summary { get; set; }
            public string Detail { get; set; }
            public string LastModifiedBy { get; set; }
        }

        #region "Security Access"
        private void SecurityDisplay()
        {
            if (uman.userSecurity != null)
            {

                this.txtbUserNamePR.Text = uman.userSecurity.abUserName;


                #region "Server Admin - must be an SQL user that has sysadmin rights..."

                bool isServerAdmin = uman.userSecurity.isServerAdmin;

                if (!isServerAdmin && uman.userSecurity.sessionPKID == 0)
                {
                    isServerAdmin = Window1.IsBuilderOrAdmin;
                }

                this.admInitDBsetup.IsEnabled = isServerAdmin;
                this.admSync.IsEnabled = isServerAdmin;

                #endregion


                #region "Sproj Admin - general form display/access"

                SecurityDisplay_AssetBuilderAdmin();

                #endregion
            }
        }
        private void SecurityDisplay_AssetBuilderAdmin()
        {
            if (uman.userSecurity.isServerAdmin || uman.userSecurity.isAssetBuilderAdmin)
            {//user resides in Sproj_Admin database role OR user has sysadmin server role
                bool granted = true;

                this.rbMangeUserRoles.IsEnabled = granted;
                this.grpUsers.IsEnabled = granted;

                this.rbCompany.IsEnabled = granted;
                this.dgvCompany.IsEnabled = granted;
                this.dgvCompanyMoveTo.IsEnabled = granted;

                this.rbNewUser.IsEnabled = granted;
            }
            else
            {//read only and user specific

                bool disabled = false;

                this.rbMangeUserRoles.IsEnabled = disabled;
                this.grpUsers.Visibility = Visibility.Collapsed;
                this.grpUserRoles.Visibility = Visibility.Collapsed;

                this.rbCompany.IsEnabled = disabled;
                this.grpCompany.Visibility = Visibility.Collapsed;
                this.grpCompanyUserMove.Visibility = Visibility.Collapsed;

                this.rbNewUser.IsEnabled = disabled;
            }
        }

        #endregion
        #region "Control Radio button Display"
        private void PageControlVisibility(bool initialLoad, RadioButton radioButton = null)
        {
            if (pageControlVisibilityEnabled)
            {

                this.grpCompany.Visibility = Visibility.Collapsed;
                this.grpUsers.Visibility = Visibility.Collapsed;
                this.grpUserRoles.Visibility = Visibility.Collapsed;
                this.grpNewUser.Visibility = Visibility.Collapsed;
                this.grpCompanyUserMove.Visibility = Visibility.Collapsed;
                this.grpMyProfile.Visibility = Visibility.Collapsed;
                this.grpMyProfileOverview.Visibility = Visibility.Collapsed;

                this.dgvUsers.SelectionMode = DataGridSelectionMode.Single;

                if (initialLoad)
                {
                    this.grpMyProfile.Visibility = Visibility.Visible;
                    this.grpMyProfileOverview.Visibility = Visibility.Visible;
                    PopulateMyProfileOverview();
                }

                if (!initialLoad && radioButton != null)
                {
                    if (radioButton == this.rbNewUser)
                    {
                        this.txtbUserName.Text = "";
                        this.txtbPassword.Text = "";
                        this.grpNewUser.Visibility = Visibility.Visible;
                        this.grpCompany.Visibility = Visibility.Visible;
                    }
                    else if (radioButton == this.rbMangeUserRoles)
                    {
                        this.grpCompany.Visibility = Visibility.Visible;
                        this.grpUsers.Visibility = Visibility.Visible;
                        this.dgvUsers.Visibility = Visibility.Visible;
                        this.grpUserRoles.Visibility = Visibility.Visible;
                    }
                    else if (radioButton == this.rbCompany)
                    {
                        this.grpUsers.Visibility = Visibility.Visible;
                        this.grpCompany.Visibility = Visibility.Visible;
                        this.grpCompanyUserMove.Visibility = Visibility.Visible;

                        this.dgvUsers.SelectionMode = DataGridSelectionMode.Extended;
                    }
                    else if (radioButton == this.rbMyProfile)
                    {
                        this.txtbPasswordPR.Text = "";
                        this.grpMyProfile.Visibility = Visibility.Visible;
                        this.grpMyProfileOverview.Visibility = Visibility.Visible;

                        PopulateMyProfileOverview();
                    }
                }
            }
        }

        private void rbNewUser_Checked(object sender, RoutedEventArgs e)
        {
            PageControlVisibility(false, (RadioButton)sender);
        }

        private void rbMangeUserRoles_Checked(object sender, RoutedEventArgs e)
        {
            PageControlVisibility(false, (RadioButton)sender);
        }

        private void rbCompany_Checked(object sender, RoutedEventArgs e)
        {
            PageControlVisibility(false, (RadioButton)sender);
        }

        private void rbMyProfile_Checked(object sender, RoutedEventArgs e)
        {
            PageControlVisibility(false, (RadioButton)sender);
        }
        #endregion
        #endregion
        #region "Toolbar"
        private void MenuItem_Click_Sync(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("You will now enforce a global synchronization between users and their roles (Please ask for advice from an administrator before continuing...)", "User Management - Sync", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                UM.Processing processing = new UM.Processing(uman.userSecurity);
                var result = processing.UserSync();
                var message = result.Message;

                if (result.IsSuccess && message == "")
                {
                    message = "Successfully synchronised users";
                }
                else if (message == "")
                {
                    message = "Failed";
                }

                MessageBox.Show(message);

                PopulateCompanies();
                PopulateCompanies_MoveTo();
                PopulateMyProfileOverview();

            }
        }

        private void MenuItem_Click_DBSetup(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("You will now execute the schematic initiliazation process that will create the database structure for user management. (Please ask for advice from an administrator before continuing...)", "User Management - Sync", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {

                if (uman.userSecurity.SchemaInitialised)
                {
                    MessageBox.Show("The database schema has already been initialsed. Please contact the system administrator to apply schema updates if required.");
                    return;
                }
                else
                {
                    UM.Processing processing = new UM.Processing(uman.userSecurity);
                    var result = processing.DBSetup();
                    var message = result.Message;

                    if (result.IsSuccess && message == "")
                    {
                        message = "Successfully initialised database schema. This module window will now close to for a refresh";

                    }
                    else if(message == "")
                    {
                        message = "Failed";
                    }

                    MessageBox.Show(message);

                    this.Close();
                }
            }
        }

        #endregion
        private void PopulateCompanies()
        {

            Company company = new Company();
            
            this.dgvCompany.ItemsSource = company.GetCompanies();
        }
        private void PopulateCompanies_MoveTo()
        {
            Company company = new Company();

            this.dgvCompanyMoveTo.ItemsSource = company.GetCompanies(companyID);
        }
        private void PopulateUsers(string spoid)
        {
            UserInfo userInfo = new UserInfo();

            this.dgvUsers.ItemsSource = userInfo.GetUsers2(spoid);
        }
        private void PopulateUserRoles(UserInfo userInfo)
        {
            this.dgvUserRoles.ItemsSource = userInfo.userRoles;

        }
        private void PopulateMyProfileOverview()
        {
            UserInfo userInfo = new UserInfo();
            UM.Uman.OverviewHistory oh = new UM.Uman.OverviewHistory(uman.userSecurity, uman.userSecurity.UserSprojoid);

            this.dgvMyProfileOverview_LoginHistory.ItemsSource = oh.UserInfoSessionLoginHistories; //userInfo.GetUserInfoSessionLoginHistories(uman.userSecurity.UserSprojoid);
            this.dgvMyProfileOverview_CurrentRoles.ItemsSource = oh.UserRoleAssignment; //userInfo.GetUserRoleAssignment(uman.userSecurity.UserSprojoid);
            this.dgvMyProfileOverview_RoleAudit.ItemsSource = oh.UserRoleAssignmentHistories; //userInfo.GetUserRoleAssignmentHistories(uman.userSecurity.UserSprojoid);
            this.dgvMyProfileOverview_Audit.ItemsSource = oh.UserAudit; //userInfo.GetUserAudits(uman.userSecurity.UserSprojoid);
            this.dgvMyProfileOverview_MyAudit.ItemsSource = oh.UserMyAudit; //userInfo.GetUserAudits(uman.userSecurity.UserSprojoid);
        }
        private void Users_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            try
            {
                DataGrid dg = (DataGrid)sender;

                if (dg.HasItems)
                {
                    if (dg.CurrentItem != null)
                    {
                        UserInfo userDetail = (UserInfo)dg.CurrentItem;
                        UserId = userDetail.Id;
                        UserSprojOid = userDetail.sprojoid;

                        PopulateUserRoles(userDetail);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        private void Companies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DataGrid dg = (DataGrid)sender;

                this.dgvUsers.ItemsSource = null;
                this.dgvUserRoles.ItemsSource = null;

                if (dg.HasItems)
                {
                    Company company = (Company)dg.CurrentItem;

                    if (company != null)
                    {
                        companyID = company.Id;
                        CompanySprojOid = company.sprojoid;

                        PopulateUsers(CompanySprojOid);
                        PopulateCompanies_MoveTo();
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }
        private void UpdateUserRoles(object sender, RoutedEventArgs e)
        {
            try
            {
                DataGrid dg = (DataGrid)this.dgvUsers;

                if (dg.HasItems)
                {
                    ItemCollection items = this.dgvUserRoles.Items;

                    UM.Processing process = new UM.Processing(uman.userSecurity);
                    UserInfo userInfo = (UserInfo)dg.SelectedItem;

                    for (var i = 0; i < items.Count; i++)
                    {
                        UserRole role = (UserRole)items[i];

                        if (role.Assigned)
                        {
                            process.UpdateUserRoles(userId:UserSprojOid, roleId: role.sprojoid, role.Assigned);
                        }
                        else
                        {
                            UM.Uman.UserRole userRole = new UM.Uman.UserRole
                            {
                                active = false,
                                assignedToRoleInSproj = role.Assigned,
                                currentDbRole = false,
                                databaseRoleID = 0,
                                roleID = role.Id,
                                roleName = role.Name,
                                sprojoid = role.sprojoid
                            };

                            process.UpdateUserRole_Remove(userId: UserSprojOid, userRole);
                        }
                    }

                    PopulateUsers(CompanySprojOid);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
        private void Button_Click_MoveUser(object sender, RoutedEventArgs e)
        {
            if (this.dgvUsers.Items.Count > 0 && this.dgvCompanyMoveTo.Items.Count > 0)
            {

                Company fromCompany = (Company)this.dgvCompany.SelectedItem;
                Company toCompany = (Company)this.dgvCompanyMoveTo.SelectedItem;

                var items = dgvUsers.SelectedItems;

                if (items.Count == 0)
                {
                    MessageBox.Show("At least one user should be selected.");
                    return;
                }

                foreach (UserInfo userDetail in dgvUsers.SelectedItems)
                {

                    if (userDetail != null && fromCompany != null && toCompany != null)
                    {
                        try
                        {
                            UM.Processing process = new UM.Processing(uman.userSecurity);
                            UserInfo userInfo = new UserInfo().GetUserDetail(userSpoid: UserSprojOid);

                            process.MoveUserEntity(userDetail.sprojoid, toCompany.sprojoid);

                        }
                        catch (Exception e1)
                        {
                            MessageBox.Show(e1.Message.ToString());
                        }
                    }
                }

                PopulateUsers(CompanySprojOid);
            }
        }
        private void Button_Click_NewUser(object sender, RoutedEventArgs e)
        {
            if (companyID > 0)
            {
                if (this.txtbUserName.Text == "" || this.txtbPassword.Text == "")
                {
                    MessageBox.Show("Please provide both username and password...");
                }
                else
                {
                    UserInfo userInfo = new UserInfo().GetUserDetail(userName: this.txtbUserName.Text);

                    if (userInfo.Id > 0 && userInfo.Name.ToLower() == this.txtbUserName.Text.ToString().ToLower())
                    {
                        MessageBox.Show($"{userInfo.Name.ToString().ToUpper()} already exists!");
                    }
                    else
                    {

                        //call process to create SQL login, add user to Robin and Comments DB...
                        UM.Processing process = new UM.Processing(uman.userSecurity);
                        UM.Processing.ProcessStatus processStatus = process.CreateNewSqlUser(CompanySprojOid, this.txtbUserName.Text, this.txtbPassword.Text);
                        int PKUserID = 0;

                        if(processStatus.Value != null)
                        {
                            PKUserID = Convert.ToInt32(processStatus.Value.IntValue);
                        }

                        if (processStatus.IsSuccess && PKUserID > 0)
                        {
                            MessageBox.Show(this.txtbUserName.Text.ToUpper() + " successfully added. Please assign user to specific Roles as required.");

                            PopulateCompanies();
                            this.rbMangeUserRoles.IsChecked = true;
                        }
                        else
                        {
                            var mess = processStatus.Message;

                            if (processStatus.IsSuccess)
                            {
                                if(mess == "") { mess = "Successfully added user"; }
                            }
                            else
                            {
                                if (mess == null || mess == "")
                                {
                                    mess = "Internal processing error!";
                                }
                            }

                            MessageBox.Show(mess);
                        }
                    }

                    PopulateUsers(CompanySprojOid);
                }
            }
            else
            {
                MessageBox.Show("Please select a company");
            }
        }
        private void Button_Click_PasswordReset(object sender, RoutedEventArgs e)
        {

            UM.Processing process = new UM.Processing(uman.userSecurity);
            process.UserPasswordReset(this.txtbUserNamePR.Text.ToString(), this.txtbPasswordPR.Text.ToString());

        }
        private void Button_Click_UserSearch(object sender, RoutedEventArgs e)
        {

        }
    }
}
