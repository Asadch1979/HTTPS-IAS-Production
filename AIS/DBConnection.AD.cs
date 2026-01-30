using AIS.Models;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace AIS.Controllers
    {
    public partial class DBConnection : Controller, IDBConnection
        {
        public List<MenuModel> GetAllTopMenus()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<MenuModel> modelList = new List<MenuModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GetAllTopMenus";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    MenuModel menu = new MenuModel();
                    menu.Menu_Id = Convert.ToInt32(rdr["MENU_ID"]);
                    menu.Menu_Name = rdr["MENU_NAME"].ToString();
                    menu.Menu_Order = rdr["MENU_ORDER"].ToString();
                    menu.Menu_Description = rdr["MENU_DESCRIPTION"].ToString();
                    modelList.Add(menu);
                    }
                }
            con.Dispose();
            return modelList;
            }

        public List<MenuPagesModel> GetAllMenuPages(int menuId = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();

            List<MenuPagesModel> modelList = new List<MenuPagesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_GetAllMenuPages";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("menuId", OracleDbType.Int32).Value = menuId;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    MenuPagesModel menuPage = new MenuPagesModel();
                    menuPage.Id = Convert.ToInt32(rdr["ID"]);
                    menuPage.Menu_Id = Convert.ToInt32(rdr["MENU_ID"]);
                    menuPage.Page_Name = rdr["PAGE_NAME"].ToString();
                    menuPage.Page_Path = rdr["PAGE_PATH"].ToString();
                    menuPage.Page_Order = Convert.ToInt32(rdr["PAGE_ORDER"]);
                    menuPage.Status = rdr["STATUS"].ToString();
                    modelList.Add(menuPage);
                    }
                }
            con.Dispose();
            return modelList;
            }

        public List<MenuPagesModel> GetAssignedMenuPages(int groupId, int menuId)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();

            List<MenuPagesModel> modelList = new List<MenuPagesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GetAssignedMenuPages";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("groupId", OracleDbType.Int32).Value = groupId;
                cmd.Parameters.Add("menuId", OracleDbType.Int32).Value = menuId;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    MenuPagesModel menuPage = new MenuPagesModel();
                    menuPage.Id = Convert.ToInt32(rdr["ID"]);
                    menuPage.Menu_Id = Convert.ToInt32(rdr["MENU_ID"]);
                    menuPage.Page_Name = rdr["PAGE_NAME"].ToString();
                    menuPage.Page_Path = rdr["PAGE_PATH"].ToString();
                    menuPage.Page_Order = Convert.ToInt32(rdr["PAGE_ORDER"]);
                    menuPage.Status = rdr["STATUS"].ToString();
                    modelList.Add(menuPage);
                    }
                }
            con.Dispose();
            return modelList;
            }

        public bool UpdateMenuPagesAssignment(int menuId, int pageId)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_updateAllMenuPages";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("menuId", OracleDbType.Int32).Value = menuId;
                cmd.Parameters.Add("p_id", OracleDbType.Int32).Value = pageId;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.ExecuteReader();
                }
            con.Dispose();
            return true;
            }

        public List<GroupModel> GetGroups()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<GroupModel> groupList = new List<GroupModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GetGroups";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    GroupModel grp = new GroupModel();
                    grp.GROUP_ID = Convert.ToInt32(rdr["GROUP_ID"]);
                    grp.GROUP_NAME = rdr["GROUP_NAME"].ToString();
                    grp.GROUP_DESCRIPTION = rdr["DESCRIPTION"].ToString();
                    grp.GROUP_CODE = Convert.ToInt32(rdr["GROUP_ID"]);
                    grp.ISACTIVE = rdr["STATUS"].ToString();
                    groupList.Add(grp);
                    }
                }
            con.Dispose();
            return groupList;
            }

        public GroupModel AddGroup(GroupModel gm)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                if (gm.GROUP_ID == 0)
                    {
                    cmd.CommandText = "pkg_ad.p_AddGroup";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("GROUP_DESCRIPTION", OracleDbType.Varchar2).Value = gm.GROUP_DESCRIPTION;
                    cmd.Parameters.Add("GROUP_NAME", OracleDbType.Varchar2).Value = gm.GROUP_NAME;
                    cmd.Parameters.Add("ISACTIVE", OracleDbType.Varchar2).Value = gm.ISACTIVE;
                    cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                    cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                    cmd.ExecuteReader();
                    }
                else if (gm.GROUP_ID != 0)
                    {
                    cmd.CommandText = "pkg_ad.P_Group_Update";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("GROUPID", OracleDbType.Varchar2).Value = gm.GROUP_ID;
                    cmd.Parameters.Add("GROUP_DESCRIPTION", OracleDbType.Varchar2).Value = gm.GROUP_DESCRIPTION;
                    cmd.Parameters.Add("GROUP_NAME", OracleDbType.Varchar2).Value = gm.GROUP_NAME;
                    cmd.Parameters.Add("ISACTIVE", OracleDbType.Varchar2).Value = gm.ISACTIVE;
                    cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                    cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                    cmd.ExecuteReader();
                    }
                }
            con.Dispose();
            return gm;
            }

        public List<RoleRespModel> GetRoleResponsibilities()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<RoleRespModel> groupList = new List<RoleRespModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GetRoleResponsibilities";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    RoleRespModel grp = new RoleRespModel();
                    grp.DESIGNATIONCODE = Convert.ToInt32(rdr["DESIGNATIONCODE"]);
                    grp.DESCRIPTION = rdr["DESCRIPTION"].ToString();
                    grp.STATUS = rdr["STATUSTYPE"].ToString();
                    groupList.Add(grp);
                    }
                }
            con.Dispose();
            return groupList;
            }

        public List<AnnexureModel> GetAnnexuresForChecklistDetail()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<AnnexureModel> groupList = new List<AnnexureModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_get_annexure";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    AnnexureModel grp = new AnnexureModel();
                    grp.ID = Convert.ToInt32(rdr["ID"]);
                    grp.ANNEX = rdr["Annex"].ToString();
                    grp.CODE = rdr["Code"].ToString();
                    grp.HEADING = rdr["HEADING"].ToString();

                    grp.RISK = rdr["Risk"].ToString();
                    grp.PROCESS = rdr["process"].ToString();
                    grp.FUNCTION_OWNER = rdr["function"].ToString();

                    grp.RISK_ID = rdr["Risk_ID"].ToString();
                    grp.PROCESS_ID = rdr["process_Id"].ToString();
                    grp.FUNCTION_OWNER_ID = rdr["function_Id"].ToString();


                    grp.FUNCTION_ID_1 = rdr["function_id_1"].ToString();
                    grp.FUNCTION_1 = rdr["function_1"].ToString();
                    grp.FUNCTION_ID_2 = rdr["function_id_2"].ToString();
                    grp.FUNCTION_2 = rdr["function_2"].ToString();

                    grp.MAX_NUMBER = rdr["max_number"].ToString();
                    grp.WEIGHTAGE = rdr["weightage"].ToString();
                    grp.GRAVITY = rdr["gravity"].ToString();

                    grp.VOL = rdr["Vol"].ToString();
                    grp.STATUS = rdr["Status"].ToString();
                    groupList.Add(grp);
                    }
                }
            con.Dispose();
            return groupList;
            }

        public List<UserModel> GetAllUsers(FindUserModel user)
            {
            List<UserModel> userList = new List<UserModel>();
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_get_allusers";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = (object?)user.ENTITYID ?? DBNull.Value;
                cmd.Parameters.Add("EMAIL", OracleDbType.Varchar2).Value = string.IsNullOrWhiteSpace(user.EMAIL) ? (object)DBNull.Value : user.EMAIL;
                cmd.Parameters.Add("GROUPID", OracleDbType.Int32).Value = (object?)user.GROUPID ?? DBNull.Value;
                cmd.Parameters.Add("PPNUMBER", OracleDbType.Int32).Value = (object?)user.PPNUMBER ?? DBNull.Value;
                cmd.Parameters.Add("LOGINNAME", OracleDbType.Varchar2).Value = string.IsNullOrWhiteSpace(user.LOGINNAME) ? (object)DBNull.Value : user.LOGINNAME;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    UserModel um = new UserModel();
                    if (rdr["USERID"].ToString() != null && rdr["USERID"].ToString() != "")
                        um.ID = Convert.ToInt32(rdr["USERID"]);
                    um.PPNumber = rdr["PPNO"].ToString();
                    um.Name = rdr["EMPLOYEEFIRSTNAME"].ToString() + " " + rdr["EMPLOYEELASTNAME"].ToString();
                    um.Email = rdr["EMAIL"].ToString();
                    if (rdr["entity_id"].ToString() != null && rdr["entity_id"].ToString() != "")
                        um.UserEntityID = Convert.ToInt32(rdr["entity_id"].ToString());

                    if (rdr["child_code"].ToString() != null && rdr["child_code"].ToString() != "")
                        um.UserEntityCode = Convert.ToInt32(rdr["child_code"].ToString());

                    if (rdr["p_type_id"].ToString() != null && rdr["p_type_id"].ToString() != "")
                        um.UserParentEntityTypeID = Convert.ToInt32(rdr["p_type_id"].ToString());

                    if (rdr["parent_id"].ToString() != null && rdr["parent_id"].ToString() != "")
                        um.UserParentEntityID = Convert.ToInt32(rdr["parent_id"].ToString());

                    if (rdr["parent_code"].ToString() != null && rdr["parent_code"].ToString() != "")
                        um.UserParentEntityCode = Convert.ToInt32(rdr["parent_code"].ToString());

                    if (rdr["c_type_id"].ToString() != null && rdr["c_type_id"].ToString() != "")
                        um.UserEntityTypeID = Convert.ToInt32(rdr["c_type_id"].ToString());

                    um.UserEntityName = rdr["c_name"].ToString();
                    if (rdr["relation_type_id"].ToString() != null && rdr["relation_type_id"].ToString() != "")
                        um.RelationshipId = Convert.ToInt32(rdr["relation_type_id"]);
                    um.UserParentEntityName = rdr["p_name"].ToString();
                    if (Convert.ToInt32(rdr["type_id"].ToString()) == 6)
                        {
                        if (rdr["code"].ToString() != null && rdr["code"].ToString() != "")
                            {
                            um.UserPostingBranch = Convert.ToInt32(rdr["code"]);
                            }
                        if (rdr["parent_code"].ToString() != null && rdr["parent_code"].ToString() != "")
                            {
                            um.UserPostingZone = Convert.ToInt32(rdr["parent_code"]);
                            }

                        }
                    else
                        {
                        if (rdr["code"].ToString() != null && rdr["code"].ToString() != "")
                            {
                            um.UserPostingDept = Convert.ToInt32(rdr["code"]);
                            }
                        if (rdr["parent_code"].ToString() != null && rdr["parent_code"].ToString() != "")
                            {
                            um.UserPostingDiv = Convert.ToInt32(rdr["parent_code"]);
                            }

                        }

                    if (rdr["group_id"].ToString() != null && rdr["group_id"].ToString() != "")
                        {
                        um.UserGroupID = Convert.ToInt32(rdr["group_id"]);
                        um.UserRoleID = Convert.ToInt32(rdr["group_id"]);
                        }


                    um.DivName = rdr["p_name"].ToString();
                    um.DeptName = rdr["c_name"].ToString();
                    um.ZoneName = rdr["p_name"].ToString();
                    um.BranchName = rdr["c_name"].ToString();
                    um.UserRole = rdr["group_name"].ToString();
                    um.UserGroup = rdr["group_name"].ToString();
                    um.IsActive = rdr["ISACTIVE"].ToString();
                    userList.Add(um);
                    }
                }
            con.Dispose();
            return userList;

            }

        public string AddNewUser(FindUserModel user)
            {
            string resp = "";
            var enc_pass = HashPassword(user.PASSWORD);
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_add_new_user";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENC_PASS", OracleDbType.Varchar2).Value = enc_pass;
                cmd.Parameters.Add("ROLE_ID", OracleDbType.Int32).Value = user.GROUPID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = user.PPNUMBER;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = user.ENTITYID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public List<AuditEntitiesModel> GetAuditEntityTypes(int page_id)
            {
            var sessionHandler = CreateSessionHandler();

            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<AuditEntitiesModel> entitiesList = new List<AuditEntitiesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_Get_Entity_type";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("PG_ID", OracleDbType.Int32).Value = page_id;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditEntitiesModel entity = new AuditEntitiesModel();
                    entity.AUTID = Convert.ToInt32(rdr["AUTID"]);
                    entity.ENTITYCODE = rdr["ENTITYCODE"].ToString();
                    entity.ENTITYTYPEDESC = rdr["ENTITYTYPEDESC"].ToString();
                    entity.AUDITABLE = rdr["AUDITABLE"].ToString();
                    //entity.D_RISK = rdr["d_risk"].ToString();
                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }

        public List<AuditEntitiesModel> GetAuditBy()
            {
            var sessionHandler = CreateSessionHandler();

            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<AuditEntitiesModel> entitiesList = new List<AuditEntitiesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_get_audited_by";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditEntitiesModel entity = new AuditEntitiesModel();
                    entity.AUTID = Convert.ToInt32(rdr["entity_id"]);
                    entity.ENTITYTYPEDESC = rdr["deptname"].ToString();

                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }

        public List<AuditeeEntitiesModel> GetAuditeeEntities(int ENTITY_TYPE_ID = 0)
            {
            string TYPE_ID = "";
            if (ENTITY_TYPE_ID != 0)
                TYPE_ID = Convert.ToString(ENTITY_TYPE_ID);
            var con = this.DatabaseConnection();
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeEntitiesModel> entitiesList = new List<AuditeeEntitiesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GetAuditeeEntityTypes";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    AuditeeEntitiesModel entity = new AuditeeEntitiesModel();
                    if (rdr["entitycode"].ToString() != "" && rdr["entitycode"].ToString() != null)
                        entity.ENTITY_ID = Convert.ToInt32(rdr["entitycode"]);

                    if (rdr["ENTITY_TYPE"].ToString() != "" && rdr["ENTITY_TYPE"].ToString() != null)
                        entity.NAME = rdr["ENTITY_TYPE"].ToString();

                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }

        public List<AuditeeEntitiesModel> GetAuditeeEntitiesForUpdate(int ENTITY_TYPE_ID = 0, int ENTITY_ID = 0)
            {
            var con = this.DatabaseConnection();
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeEntitiesModel> entitiesList = new List<AuditeeEntitiesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GetEntitees_for_update";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("TYPEID", OracleDbType.Int32).Value = ENTITY_TYPE_ID;
                cmd.Parameters.Add("RO_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    AuditeeEntitiesModel entity = new AuditeeEntitiesModel();

                    if (rdr["ENTITY_ID"].ToString() != "" && rdr["ENTITY_ID"].ToString() != null)
                        entity.ENTITY_ID = Convert.ToInt32(rdr["ENTITY_ID"]);

                    if (rdr["CODE"].ToString() != "" && rdr["CODE"].ToString() != null)
                        entity.CODE = Convert.ToInt32(rdr["CODE"]);

                    if (rdr["NAME"].ToString() != "" && rdr["NAME"].ToString() != null)
                        entity.NAME = rdr["NAME"].ToString();

                    entity.ACTIVE = rdr["ACTIVE"].ToString();

                    if (rdr["AUDITBY_ID"].ToString() != "" && rdr["AUDITBY_ID"].ToString() != null)
                        entity.AUDITBY_ID = Convert.ToInt32(rdr["AUDITBY_ID"]);

                    entity.AUDITBY_NAME = rdr["AUDITBY_NAME"].ToString();
                    entity.AUDITABLE = rdr["AUDITABLE"].ToString();
                    entity.ADDRESS = rdr["ADDRESS"].ToString();
                    entity.TELEPHONE = rdr["TELEPHONE"].ToString();
                    entity.EMAIL_ADDRESS = rdr["EMAIL_ADDRESS"].ToString();
                    if (rdr["RISK_ID"].ToString() != "" && rdr["RISK_ID"].ToString() != null)
                        entity.RISK_ID = Convert.ToInt32(rdr["RISK_ID"]);
                    if (rdr["SIZE_ID"].ToString() != "" && rdr["SIZE_ID"].ToString() != null)
                        entity.SIZE_ID = Convert.ToInt32(rdr["SIZE_ID"]);
                    entity.ERISK = rdr["ERISK"].ToString();
                    entity.ESIZE = rdr["ESIZE"].ToString();
                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }

        public List<AuditeeEntityUpdateModel> GetAuditeeEntitiesForUpdateForAuthorization(int ENTITY_TYPE_ID = 0, int ENTITY_ID = 0)
            {
            var con = this.DatabaseConnection();
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeEntityUpdateModel> entitiesList = new List<AuditeeEntityUpdateModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GetEntitees_for_update_comp";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("E_ENTITY_ID", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserGroupID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    AuditeeEntityUpdateModel entity = new AuditeeEntityUpdateModel();

                    if (rdr["ENTITY_ID"].ToString() != "" && rdr["ENTITY_ID"].ToString() != null)
                        entity.ENTITY_ID = Convert.ToInt32(rdr["ENTITY_ID"]);

                    if (rdr["CODE"].ToString() != "" && rdr["CODE"].ToString() != null)
                        entity.CODE = Convert.ToInt32(rdr["CODE"]);

                    if (rdr["CODE_OLD"].ToString() != "" && rdr["CODE_OLD"].ToString() != null)
                        entity.CODE_OLD = Convert.ToInt32(rdr["CODE_OLD"]);

                    if (rdr["NAME"].ToString() != "" && rdr["NAME"].ToString() != null)
                        entity.NAME = rdr["NAME"].ToString();

                    if (rdr["NAME_OLD"].ToString() != "" && rdr["NAME_OLD"].ToString() != null)
                        entity.NAME_OLD = rdr["NAME_OLD"].ToString();

                    entity.ACTIVE = rdr["ACTIVE"].ToString();
                    entity.ACTIVE_OLD = rdr["ACTIVE_OLD"].ToString();

                    if (rdr["AUDITBY_ID"].ToString() != "" && rdr["AUDITBY_ID"].ToString() != null)
                        entity.AUDITBY_ID = Convert.ToInt32(rdr["AUDITBY_ID"]);

                    if (rdr["AUDITBY_ID_OLD"].ToString() != "" && rdr["AUDITBY_ID_OLD"].ToString() != null)
                        entity.AUDITBY_ID_OLD = Convert.ToInt32(rdr["AUDITBY_ID_OLD"]);

                    entity.AUDITBY_NAME = rdr["AUDITBY_NAME"].ToString();
                    entity.AUDITBY_NAME_OLD = rdr["AUDITBY_NAME_OLD"].ToString();

                    entity.AUDITABLE = rdr["AUDITABLE"].ToString();
                    entity.AUDITABLE_OLD = rdr["AUDITABLE_OLD"].ToString();

                    entity.ADDRESS = rdr["ADDRESS"].ToString();
                    entity.ADDRESS_OLD = rdr["ADDRESS_OLD"].ToString();

                    entity.TELEPHONE = rdr["TELEPHONE"].ToString();
                    entity.TELEPHONE_OLD = rdr["TELEPHONE_OLD"].ToString();

                    entity.EMAIL_ADDRESS = rdr["EMAIL_ADDRESS"].ToString();
                    entity.EMAIL_ADDRESS_OLD = rdr["EMAIL_ADDRESS_OLD"].ToString();


                    if (rdr["RISK_ID"].ToString() != "" && rdr["RISK_ID"].ToString() != null)
                        entity.RISK_ID = Convert.ToInt32(rdr["RISK_ID"]);

                    if (rdr["RISK_ID_OLD"].ToString() != "" && rdr["RISK_ID_OLD"].ToString() != null)
                        entity.RISK_ID_OLD = Convert.ToInt32(rdr["RISK_ID_OLD"]);

                    if (rdr["SIZE_ID"].ToString() != "" && rdr["SIZE_ID"].ToString() != null)
                        entity.SIZE_ID = Convert.ToInt32(rdr["SIZE_ID"]);

                    if (rdr["SIZE_ID_OLD"].ToString() != "" && rdr["SIZE_ID_OLD"].ToString() != null)
                        entity.SIZE_ID_OLD = Convert.ToInt32(rdr["SIZE_ID_OLD"]);

                    entity.ERISK = rdr["ERISK"].ToString();
                    entity.ERISK_OLD = rdr["ERISK_OLD"].ToString();

                    entity.ESIZE = rdr["ESIZE"].ToString();
                    entity.ESIZE_OLD = rdr["ESIZE_OLD"].ToString();

                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }

        public List<AuditeeEntityUpdateModel> GetAuditeeEntitiesForAuthorization()
            {
            var con = this.DatabaseConnection();
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeEntityUpdateModel> entitiesList = new List<AuditeeEntityUpdateModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GetEntitees_for_update_authorization";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("E_ENTITY_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("E_up_status", OracleDbType.Varchar2).Value = "U";
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserGroupID;
                cmd.Parameters.Add("IND", OracleDbType.Varchar2).Value = "";
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    AuditeeEntityUpdateModel entity = new AuditeeEntityUpdateModel();
                    if (rdr["ID"].ToString() != "" && rdr["ID"].ToString() != null)
                        entity.ID = Convert.ToInt32(rdr["ID"]);
                    if (rdr["ENTITY_ID"].ToString() != "" && rdr["ENTITY_ID"].ToString() != null)
                        entity.ENTITY_ID = Convert.ToInt32(rdr["ENTITY_ID"]);
                    if (rdr["CODE"].ToString() != "" && rdr["CODE"].ToString() != null)
                        entity.CODE = Convert.ToInt32(rdr["CODE"]);
                    entity.NAME = rdr["NAME"].ToString();
                    entity.ACTIVE = rdr["ACTIVE"].ToString();
                    if (rdr["AUDITBY_ID"].ToString() != "" && rdr["AUDITBY_ID"].ToString() != null)
                        entity.AUDITBY_ID = Convert.ToInt32(rdr["AUDITBY_ID"]);
                    entity.AUDITBY_NAME = rdr["AUDITBY_NAME"].ToString();
                    entity.AUDITABLE = rdr["AUDITABLE"].ToString();
                    entity.ADDRESS = rdr["ADDRESS"].ToString();
                    entity.TELEPHONE = rdr["TELEPHONE"].ToString();
                    entity.EMAIL_ADDRESS = rdr["EMAIL_ADDRESS"].ToString();
                    if (rdr["RISK_ID"].ToString() != "" && rdr["RISK_ID"].ToString() != null)
                        entity.RISK_ID = Convert.ToInt32(rdr["RISK_ID"]);
                    if (rdr["SIZE_ID"].ToString() != "" && rdr["SIZE_ID"].ToString() != null)
                        entity.SIZE_ID = Convert.ToInt32(rdr["SIZE_ID"]);
                    entity.UP_STATUS = rdr["UP_STATUS"].ToString();
                    entity.ERISK = rdr["ERISK"].ToString();
                    entity.ESIZE = rdr["ESIZE"].ToString();
                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }

        public string UpdateAuditeeEntity(AuditeeEntityUpdateModel entityModel, string IND)
            {
            var con = this.DatabaseConnection();
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_UPDATE_ENTITIES";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("E_entity_id", OracleDbType.Int32).Value = entityModel.ENTITY_ID;
                cmd.Parameters.Add("E_code", OracleDbType.Int32).Value = entityModel.CODE;
                cmd.Parameters.Add("E_name", OracleDbType.Varchar2).Value = entityModel.NAME;
                cmd.Parameters.Add("E_active", OracleDbType.Varchar2).Value = entityModel.ACTIVE;
                cmd.Parameters.Add("E_auditable", OracleDbType.Varchar2).Value = entityModel.AUDITABLE;
                cmd.Parameters.Add("E_address", OracleDbType.Varchar2).Value = entityModel.ADDRESS;
                cmd.Parameters.Add("E_telephone", OracleDbType.Varchar2).Value = entityModel.TELEPHONE;
                cmd.Parameters.Add("E_email_address", OracleDbType.Varchar2).Value = entityModel.EMAIL_ADDRESS;
                cmd.Parameters.Add("E_risk_id", OracleDbType.Int32).Value = entityModel.RISK_ID;
                cmd.Parameters.Add("E_size_id", OracleDbType.Int32).Value = entityModel.SIZE_ID;
                cmd.Parameters.Add("E_up_status", OracleDbType.Varchar2).Value = entityModel.UP_STATUS;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserGroupID;
                cmd.Parameters.Add("IND", OracleDbType.Varchar2).Value = IND;
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public List<AuditeeEntitiesModel> GetAISEntities(string ENTITY_ID, string TYPE_ID)
            {
            var con = this.DatabaseConnection();
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeEntitiesModel> entitiesList = new List<AuditeeEntitiesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_get_auditee_entities";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Varchar2).Value = ENTITY_ID;
                cmd.Parameters.Add("T_ID", OracleDbType.Varchar2).Value = TYPE_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    AuditeeEntitiesModel entity = new AuditeeEntitiesModel();
                    if (rdr["ENTITY_ID"].ToString() != "" && rdr["ENTITY_ID"].ToString() != null)
                        entity.ENTITY_ID = Convert.ToInt32(rdr["ENTITY_ID"]);

                    entity.NAME = rdr["NAME"].ToString();
                    if (rdr["CODE"].ToString() != "" && rdr["CODE"].ToString() != null)
                        entity.CODE = Convert.ToInt32(rdr["CODE"]);

                    if (rdr["TYPE_ID"].ToString() != "" && rdr["TYPE_ID"].ToString() != null)
                        entity.TYPE_ID = Convert.ToInt32(rdr["TYPE_ID"]);
                    if (rdr["AUDITBY_ID"].ToString() != "" && rdr["AUDITBY_ID"].ToString() != null)
                        entity.AUDITBY_ID = Convert.ToInt32(rdr["AUDITBY_ID"]);

                    entity.AUDITBY_NAME = rdr["auditby_name"].ToString();
                    entity.TYPE_NAME = rdr["TYPE_NAME"].ToString();
                    entity.AUDITABLE = rdr["auditable"].ToString();
                    entity.COST_CENTER = rdr["cost_center"].ToString();
                    entity.STATUS = rdr["active"].ToString();

                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }

        public List<AuditeeEntitiesModel> GetCBASEntities(string E_CODE, string E_NAME)
            {
            var con = this.DatabaseConnection();
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeEntitiesModel> entitiesList = new List<AuditeeEntitiesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GET_CBAS_ENTITIES";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_CODE", OracleDbType.Varchar2).Value = E_CODE;
                cmd.Parameters.Add("ENT_NAME", OracleDbType.Varchar2).Value = E_NAME;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    AuditeeEntitiesModel entity = new AuditeeEntitiesModel();
                    if (rdr["ORG_UNITID"].ToString() != "" && rdr["ORG_UNITID"].ToString() != null)
                        entity.ENTITY_ID = Convert.ToInt32(rdr["ORG_UNITID"]);

                    if (rdr["NAME"].ToString() != "" && rdr["NAME"].ToString() != null)
                        entity.NAME = rdr["NAME"].ToString();

                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }

        public List<AuditeeEntitiesModel> GetERPEntities(string E_CODE, string E_NAME)
            {
            var con = this.DatabaseConnection();
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeEntitiesModel> entitiesList = new List<AuditeeEntitiesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GET_ERP_ENTITIES";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_CODE", OracleDbType.Varchar2).Value = E_CODE;
                cmd.Parameters.Add("ENT_NAME", OracleDbType.Varchar2).Value = E_NAME;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    AuditeeEntitiesModel entity = new AuditeeEntitiesModel();
                    if (rdr["ORG_ID"].ToString() != "" && rdr["ORG_ID"].ToString() != null)
                        entity.ENTITY_ID = Convert.ToInt32(rdr["ORG_ID"]);

                    if (rdr["ORG_DESC"].ToString() != "" && rdr["ORG_DESC"].ToString() != null)
                        entity.NAME = rdr["ORG_DESC"].ToString();

                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }

        public List<AuditeeEntitiesModel> GetHREntities(string E_CODE, string E_NAME)
            {
            var con = this.DatabaseConnection();
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeEntitiesModel> entitiesList = new List<AuditeeEntitiesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GET_HR_ENTITIES";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_CODE", OracleDbType.Varchar2).Value = E_CODE;
                cmd.Parameters.Add("ENT_NAME", OracleDbType.Varchar2).Value = E_NAME;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    AuditeeEntitiesModel entity = new AuditeeEntitiesModel();
                    if (rdr["div_code"].ToString() != "" && rdr["div_code"].ToString() != null)
                        entity.ENTITY_ID = Convert.ToInt32(rdr["div_code"]);

                    if (rdr["div_name"].ToString() != "" && rdr["div_name"].ToString() != null)
                        entity.NAME = rdr["div_name"].ToString();

                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }

        public AuditEntitiesModel AddAuditEntity(AuditEntitiesModel am)
            {
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_AddAuditEntity";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("AUDITABLE", OracleDbType.Varchar2).Value = am.AUDITABLE;
                cmd.Parameters.Add("ENTITYTYPEDESC", OracleDbType.Varchar2).Value = am.ENTITYTYPEDESC;
                cmd.ExecuteReader();
                }
            con.Dispose();
            return am;

            }

        public List<AuditSubEntitiesModel> GetAuditSubEntities()
            {
            List<AuditSubEntitiesModel> subEntitiesList = new List<AuditSubEntitiesModel>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GetAuditSubEntities";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditSubEntitiesModel entity = new AuditSubEntitiesModel();
                    entity.ID = Convert.ToInt32(rdr["ID"]);
                    entity.DIV_ID = Convert.ToInt32(rdr["DIV_ID"]);
                    entity.DEP_ID = Convert.ToInt32(rdr["DEP_ID"]);
                    entity.NAME = rdr["E_NAME"].ToString();
                    entity.STATUS = rdr["STATUS"].ToString();
                    subEntitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return subEntitiesList;

            }

        public UpdateUserModel UpdateUser(UpdateUserModel user)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var newPassword = "";
            bool setPassword = false;
            if (user.PASSWORD != "" && user.PASSWORD != null)
                {
                newPassword = HashPassword(user.PASSWORD);
                setPassword = !setPassword;
                }

            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.UPDATE_USERS";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("PPNUMBER", OracleDbType.Int32).Value = user.PPNO;
                if (setPassword)
                    cmd.Parameters.Add("PASS", OracleDbType.Varchar2).Value = newPassword;
                else
                    cmd.Parameters.Add("PASS", OracleDbType.Varchar2).Value = newPassword;
                cmd.Parameters.Add("IS_ACTIVE", OracleDbType.Varchar2).Value = user.ISACTIVE;
                cmd.Parameters.Add("ROLEID", OracleDbType.Int32).Value = user.ROLE_ID;
                cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = user.ENTITY_ID;
                cmd.Parameters.Add("EMAIL_ADDRESS", OracleDbType.Varchar2).Value = user.EMAIL_ADDRESS;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                cmd.ExecuteReader();
                }
            con.Dispose();
            user.PASSWORD = "";
            return user;
            }

        public PasswordResetResult ResetUserPassword(string PPNumber, string CNICNumber, string generatedPassword = null)
            {
            // Auth bypass allowed here because this method is called before session creation / for session cleanup.
            var con = this.DatabaseConnection(requireActiveSession: false);
            string pass = string.IsNullOrWhiteSpace(generatedPassword) ? this.GeneratePassword() : generatedPassword;
            string enc_pass = HashPassword(pass);
            string res = "";
            string userEmail = "";
            string userCCEmail = "";
            string userFullName = "";
            string successIndicator = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.RESET_USER_PASSWORD";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("PPNUMBER", OracleDbType.Int32).Value = PPNumber;
                cmd.Parameters.Add("CNIC", OracleDbType.Int32).Value = CNICNumber;
                cmd.Parameters.Add("PASS", OracleDbType.Varchar2).Value = enc_pass;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    res = rdr["remarks"].ToString();
                    userEmail = rdr["emailAddress"].ToString();
                    userCCEmail = rdr["emailAddress2"].ToString();
                    userFullName = rdr["empFullName"].ToString();
                    successIndicator = rdr["IND"].ToString();
                    }
                }
            con.Dispose();

            var result = new PasswordResetResult
                {
                Message = res,
                AccountFound = string.Equals(successIndicator, "Y", StringComparison.OrdinalIgnoreCase),
                PasswordReset = string.Equals(successIndicator, "Y", StringComparison.OrdinalIgnoreCase)
                };

            if (result.AccountFound)
                {
                EmailNotification.SendPasswordResetSuccess(_configuration, userFullName, PPNumber, pass, userEmail, userCCEmail, _httpCon?.HttpContext?.RequestServices);
                result.EmailSent = true;
                }

            return result;

            }

        public void AddGroupMenuItemsAssignment(int group_id = 0, int menu_item_id = 0)
            {
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_AddGroupMenuItemsAssignment";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("groupid", OracleDbType.Int32).Value = group_id;
                cmd.Parameters.Add("PAGEID", OracleDbType.Int32).Value = menu_item_id;
                cmd.ExecuteReader();
                }
            con.Dispose();
            }

        public void RemoveGroupMenuItemsAssignment(int group_id = 0, int menu_item_id = 0)
            {
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_RemoveGroupMenuItemsAssignment";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("groupid", OracleDbType.Int32).Value = group_id;
                cmd.Parameters.Add("PAGEID", OracleDbType.Int32).Value = menu_item_id;
                cmd.ExecuteReader();
                }
            con.Dispose();
            }

        public List<AuditZoneModel> GetAuditZones(bool sessionCheck = true)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            List<AuditZoneModel> AZList = new List<AuditZoneModel>();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            int entityId = 0;
            if (loggedInUser.UserGroupID != 1)
                {
                if (sessionCheck)
                    entityId = Convert.ToInt32(loggedInUser.PPNumber);
                }
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GetAuditZones";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = entityId;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditZoneModel z = new AuditZoneModel();
                    z.ID = Convert.ToInt32(rdr["entity_id"]);
                    z.ZONECODE = rdr["CODE"].ToString();
                    z.ZONENAME = rdr["NAME"].ToString();
                    z.DESCRIPTION = rdr["DESCRIPTION"].ToString();
                    if (rdr["ACTIVE"].ToString() == "A")
                        z.ISACTIVE = "Active";
                    else if (rdr["ACTIVE"].ToString() == "I")
                        z.ISACTIVE = "InActive";
                    else
                        z.ISACTIVE = rdr["ACTIVE"].ToString();

                    AZList.Add(z);
                    }
                }
            con.Dispose();
            return AZList;
            }

        public List<InspectionUnitsModel> GetInspectionUnits()
            {
            var con = this.DatabaseConnection();
            List<InspectionUnitsModel> ICList = new List<InspectionUnitsModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {

                cmd.CommandText = "pkg_ad.P_GetInspectionUnits";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    InspectionUnitsModel z = new InspectionUnitsModel();
                    z.I_ID = Convert.ToInt32(rdr["I_ID"]);
                    z.I_CODE = rdr["I_CODE"].ToString();
                    z.UNIT_NAME = rdr["UNIT_NAME"].ToString();
                    z.DISCRIPTION = rdr["DISCRIPTION"].ToString();
                    if (rdr["STATUS"].ToString() == "Y")
                        z.STATUS = "Active";
                    else if (rdr["STATUS"].ToString() == "N")
                        z.STATUS = "InActive";
                    else
                        z.STATUS = rdr["ISACTIVE"].ToString();

                    ICList.Add(z);
                    }
                }
            con.Dispose();
            return ICList;
            }

        public List<BranchModel> GetBranches(int zone_code = 0, bool sessionCheck = true)
            {
            var con = this.DatabaseConnection();
            List<BranchModel> branchList = new List<BranchModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GetBranches";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("Zone_Id", OracleDbType.Int32).Value = zone_code;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    BranchModel br = new BranchModel();
                    br.BRANCHID = Convert.ToInt32(rdr["BRANCHID"]);
                    br.ZONEID = Convert.ToInt32(rdr["ZONEID"]);
                    br.BRANCHNAME = rdr["BRANCHNAME"].ToString();
                    br.ZONE_NAME = rdr["ZONENAME"].ToString();
                    br.BRANCHCODE = rdr["BRANCHCODE"].ToString();
                    br.BRANCH_SIZE_ID = 1;
                    br.DESCRIPTION = rdr["DESCRIPTION"].ToString();
                    br.BRANCH_SIZE = "";
                    if (rdr["ISACTIVE"].ToString() == "Y")
                        br.ISACTIVE = "Active";
                    else if (rdr["ISACTIVE"].ToString() == "N")
                        br.ISACTIVE = "InActive";
                    else
                        br.ISACTIVE = rdr["ISACTIVE"].ToString();

                    branchList.Add(br);
                    }
                }
            con.Dispose();
            return branchList;
            }

        public List<ZoneModel> GetZones(bool sessionCheck = true)
            {
            var con = this.DatabaseConnection();
            List<ZoneModel> zoneList = new List<ZoneModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GetZones";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ZoneModel z = new ZoneModel();
                    z.ZONEID = Convert.ToInt32(rdr["ZONEID"]);
                    z.ENTITYID = Convert.ToInt32(rdr["ENTITY_ID"]);
                    z.ZONECODE = rdr["ZONECODE"].ToString();
                    z.ZONENAME = rdr["NAME"].ToString();
                    z.DESCRIPTION = rdr["DESCRIPTION"].ToString();
                    if (rdr["ISACTIVE"].ToString() == "Y")
                        z.ISACTIVE = "Active";
                    else if (rdr["ISACTIVE"].ToString() == "N")
                        z.ISACTIVE = "InActive";
                    else
                        z.ISACTIVE = rdr["ISACTIVE"].ToString();

                    zoneList.Add(z);
                    }
                }
            con.Dispose();
            return zoneList;
            }

        public List<ZoneModel> GetZonesoldparamointoring(bool sessionCheck = true)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<ZoneModel> zoneList = new List<ZoneModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GetZonesForHoMointoring";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ZoneModel z = new ZoneModel();
                    z.ZONEID = Convert.ToInt32(rdr["ZONEID"]);
                    z.ENTITYID = Convert.ToInt32(rdr["ENTITY_ID"]);
                    z.ZONECODE = rdr["ZONECODE"].ToString();
                    z.ZONENAME = rdr["NAME"].ToString();
                    z.DESCRIPTION = rdr["DESCRIPTION"].ToString();
                    if (rdr["ISACTIVE"].ToString() == "Y")
                        z.ISACTIVE = "Active";
                    else if (rdr["ISACTIVE"].ToString() == "N")
                        z.ISACTIVE = "InActive";
                    else
                        z.ISACTIVE = rdr["ISACTIVE"].ToString();

                    zoneList.Add(z);
                    }
                }
            con.Dispose();
            return zoneList;
            }

        public List<BranchSizeModel> GetBranchSizes()
            {
            var con = this.DatabaseConnection();
            List<BranchSizeModel> brSizeList = new List<BranchSizeModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GetBranchSizes";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    BranchSizeModel bs = new BranchSizeModel();
                    bs.BR_SIZE_ID = Convert.ToInt32(rdr["ENTITY_SIZE"]);
                    bs.DESCRIPTION = rdr["DESCRIPTION"].ToString();

                    brSizeList.Add(bs);
                    }
                }
            con.Dispose();
            return brSizeList;
            }

        public List<ControlViolationsModel> GetControlViolations()
            {
            var con = this.DatabaseConnection();
            List<ControlViolationsModel> controlViolationList = new List<ControlViolationsModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GetControlViolations";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ControlViolationsModel v = new ControlViolationsModel();
                    v.ID = Convert.ToInt32(rdr["S_GR_ID"]);
                    v.V_NAME = rdr["DESCRIPTION"].ToString();
                    if (rdr["MAX_NUMBER"].ToString() != null && rdr["MAX_NUMBER"].ToString() != "")
                        v.MAX_NUMBER = Convert.ToInt32(rdr["MAX_NUMBER"]);
                    v.STATUS = "Y";
                    controlViolationList.Add(v);
                    }
                }
            con.Dispose();
            return controlViolationList;
            }

        public List<SubEntitiesModel> GetSubEntities(int div_code = 0, int dept_code = 0)
            {
            var con = this.DatabaseConnection();
            List<SubEntitiesModel> entitiesList = new List<SubEntitiesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GetSubEntities";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("dept_code", OracleDbType.Varchar2).Value = dept_code;
                cmd.Parameters.Add("Div_id", OracleDbType.Varchar2).Value = div_code;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    SubEntitiesModel entity = new SubEntitiesModel();
                    entity.ID = Convert.ToInt32(rdr["ID"]);
                    entity.DIV_ID = Convert.ToInt32(rdr["DIV_ID"]);
                    entity.DEP_ID = Convert.ToInt32(rdr["DEP_ID"]);
                    entity.NAME = rdr["NAME"].ToString();
                    entity.Division_Name = rdr["DIV_NAME"].ToString();
                    entity.Department_Name = rdr["DEPT_NAME"].ToString();
                    if (rdr["STATUS"].ToString() == "Y")
                        entity.STATUS = "Active";
                    else if (rdr["STATUS"].ToString() == "N")
                        entity.STATUS = "InActive";
                    else
                        entity.STATUS = rdr["STATUS"].ToString();
                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;
            }

        public SubEntitiesModel AddSubEntity(SubEntitiesModel subentity)
            {
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_AddSubEntity";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("NAME", OracleDbType.Varchar2).Value = subentity.NAME;
                cmd.Parameters.Add("DIV_ID", OracleDbType.Int32).Value = subentity.DIV_ID;
                cmd.Parameters.Add("DEP_ID", OracleDbType.Int32).Value = subentity.DEP_ID;
                cmd.Parameters.Add("STATUS", OracleDbType.Varchar2).Value = subentity.STATUS;

                cmd.ExecuteReader();
                }
            con.Dispose();
            return subentity;
            }

        public SubEntitiesModel UpdateSubEntity(SubEntitiesModel subentity)
            {
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_UpdateSubEntity";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("E_id", OracleDbType.Int32).Value = subentity.ID;
                cmd.Parameters.Add("NAME", OracleDbType.Varchar2).Value = subentity.NAME;
                cmd.Parameters.Add("DIV_ID", OracleDbType.Int32).Value = subentity.DIV_ID;
                cmd.Parameters.Add("DEP_ID", OracleDbType.Int32).Value = subentity.DEP_ID;
                cmd.Parameters.Add("STATUS", OracleDbType.Varchar2).Value = subentity.STATUS;

                cmd.ExecuteReader();

                }
            con.Dispose();
            return subentity;
            }

        public List<AuditRefEngagementPlanModel> GetAuditOngoingEngagementPlans()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<AuditRefEngagementPlanModel> list = new List<AuditRefEngagementPlanModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_get_audit_team_postchanges";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader ardr = cmd.ExecuteReader();
                while (ardr.Read())
                    {
                    AuditRefEngagementPlanModel eng = new AuditRefEngagementPlanModel();
                    eng.ENG_ID = Convert.ToInt32(ardr["eng_id"].ToString());
                    eng.TEAM_NAME = ardr["team_name"].ToString();
                    eng.ENTITY_NAME = ardr["name"].ToString();
                    eng.AUDIT_STARTDATE = Convert.ToDateTime(ardr["audit_startdate"].ToString()).ToString("dd/MM/yyyy");
                    eng.AUDIT_ENDDATE = Convert.ToDateTime(ardr["audit_enddate"].ToString()).ToString("dd/MM/yyyy");
                    eng.OP_STARTDATE = Convert.ToDateTime(ardr["op_startdate"].ToString()).ToString("dd/MM/yyyy");
                    eng.OP_ENDDATE = Convert.ToDateTime(ardr["op_enddate"].ToString()).ToString("dd/MM/yyyy");
                    eng.ENTITY_ID = Convert.ToInt32(ardr["entity_id"].ToString());
                    list.Add(eng);
                    }
                }
            con.Dispose();
            return list;
            }

        public UserModel GetMatchedPPNumbers(string PPNO)
            {
            UserModel um = new UserModel();
            if (PPNO == "")
                return um;
            if (PPNO != null && PPNO != "")
                {
                var con = this.DatabaseConnection();

                using (OracleCommand cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_ad.p_get_allusers";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = 0;
                    cmd.Parameters.Add("EMAIL", OracleDbType.Varchar2).Value = "";
                    cmd.Parameters.Add("GROUPID", OracleDbType.Int32).Value = 0;
                    cmd.Parameters.Add("PPNUMBER", OracleDbType.Int32).Value = PPNO;
                    cmd.Parameters.Add("LOGINNAME", OracleDbType.Varchar2).Value = "";
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                        {
                        um.ID = Convert.ToInt32(rdr["USERID"].ToString());
                        um.Name = rdr["EMPLOYEEFIRSTNAME"].ToString() + " " + rdr["EMPLOYEELASTNAME"].ToString();
                        um.PPNumber = rdr["ppno"].ToString();
                        }
                    }
                con.Dispose();
                }

            return um;
            }

        public List<RiskProcessDetails> GetRiskProcessDetails(int procId = 0)
            {
            var con = this.DatabaseConnection();
            List<RiskProcessDetails> riskProcList = new List<RiskProcessDetails>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GetRiskProcessDetails";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("procId", OracleDbType.Int32).Value = procId;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    RiskProcessDetails pdetail = new RiskProcessDetails();
                    pdetail.ID = Convert.ToInt32(rdr["S_ID"]);
                    pdetail.P_ID = Convert.ToInt32(rdr["T_ID"]);
                    pdetail.ENTITY_TYPE = Convert.ToInt32(rdr["ENTITY_TYPE"]);
                    pdetail.TITLE = rdr["HEADING"].ToString();
                    pdetail.ACTIVE = rdr["STATUS"].ToString();
                    riskProcList.Add(pdetail);
                    }
                }
            con.Dispose();
            return riskProcList;
            }

        public List<SubProcessUpdateModelForReviewAndAuthorizeModel> GetSubChecklistComparisonDetailById(int SUB_PROCESS_ID = 0)
            {
            var con = this.DatabaseConnection();
            List<SubProcessUpdateModelForReviewAndAuthorizeModel> riskTransList = new List<SubProcessUpdateModelForReviewAndAuthorizeModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_get_sub_checklist_update_byid";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("SId", OracleDbType.Int32).Value = SUB_PROCESS_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    SubProcessUpdateModelForReviewAndAuthorizeModel pm = new SubProcessUpdateModelForReviewAndAuthorizeModel();
                    pm.PROCESS_NAME = rdr["Process"].ToString();
                    pm.SUB_PROCESS_NAME = rdr["sub_porcess"].ToString();
                    pm.NEW_PROCESS_NAME = rdr["New_Process"].ToString();
                    pm.COMMENTS = rdr["Comments"].ToString();
                    pm.NEW_SUB_PROCESS_NAME = rdr["new_sub_process"].ToString();
                    pm.P_ID = rdr["t_id"].ToString();
                    pm.NEW_P_ID = rdr["n_t_id"].ToString();
                    pm.SP_ID = rdr["s_id"].ToString();
                    pm.NEW_SP_ID = rdr["n_s_id"].ToString();
                    riskTransList.Add(pm);
                    }
                }
            con.Dispose();
            return riskTransList;
            }

        public List<ChecklistDetailComparisonModel> GetChecklistComparisonDetailById(int CHECKLIST_DETAIL_ID = 0)
            {
            var con = this.DatabaseConnection();
            List<ChecklistDetailComparisonModel> riskTransList = new List<ChecklistDetailComparisonModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_get_checklist_update_byid";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("CD_Id", OracleDbType.Int32).Value = CHECKLIST_DETAIL_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ChecklistDetailComparisonModel pTran = new ChecklistDetailComparisonModel();
                    pTran.ID = Convert.ToInt32(rdr["ID"]);
                    pTran.PROCESS = rdr["PROCESS"].ToString();
                    pTran.SUB_PROCESS = rdr["SUB_PROCESS"].ToString();
                    pTran.NEW_SUB_PROCESS = rdr["NEW_SUB_PROCESS"].ToString();

                    pTran.PROCESS_DETAIL = rdr["Check_list"].ToString();
                    pTran.NEW_PROCESS_DETAIL = rdr["NEW_Check_list"].ToString();

                    pTran.VIOLATION = rdr["voilation"].ToString();
                    pTran.NEW_VIOLATION = rdr["NEW_voilation"].ToString();

                    pTran.FUNCTIONAL_RESP = rdr["funtional_responible"].ToString();
                    pTran.NEW_FUNCTIONAL_RESP = rdr["NEW_funtional_responible"].ToString();

                    pTran.ROLE_RESP = rdr["Role_responsible"].ToString();
                    pTran.NEW_ROLE_RESP = rdr["NEW_Role_responsible"].ToString();

                    pTran.RISK = rdr["RISK"].ToString();
                    pTran.NEW_RISK = rdr["NEW_RISK"].ToString();

                    pTran.ANNEXURE = rdr["annexure"].ToString();
                    pTran.NEW_ANNEXURE = rdr["NEW_annexure"].ToString();
                    pTran.STATUS = rdr["STATUS"].ToString();

                    pTran.N_S_ID = rdr["n_s_id"].ToString();
                    pTran.N_D_ID = rdr["n_d_id"].ToString();
                    pTran.N_ROLE_RESP_ID = rdr["n_role_resp_id"].ToString();
                    pTran.N_OWNER_ID = rdr["n_owner_enitity_id"].ToString();
                    pTran.N_RISK_ID = rdr["n_risk_id"].ToString();
                    pTran.N_V_ID = rdr["n_v_id"].ToString();
                    pTran.N_ANNEX_ID = rdr["n_annex"].ToString();

                    riskTransList.Add(pTran);
                    }
                }
            con.Dispose();
            return riskTransList;
            }

        public List<ChecklistDetailComparisonModel> GetChecklistComparisonDetailByIdForRefferedBack(int CHECKLIST_DETAIL_ID = 0)
            {
            var con = this.DatabaseConnection();
            List<ChecklistDetailComparisonModel> riskTransList = new List<ChecklistDetailComparisonModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_get_checklist_update_byid_ref";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("CD_Id", OracleDbType.Int32).Value = CHECKLIST_DETAIL_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ChecklistDetailComparisonModel pTran = new ChecklistDetailComparisonModel();
                    pTran.ID = Convert.ToInt32(rdr["ID"]);

                    pTran.PROCESS = rdr["PROCESS"].ToString();
                    pTran.PROCESS_ID = rdr["PROCESS_ID"].ToString();

                    pTran.SUB_PROCESS = rdr["SUB_PROCESS"].ToString();
                    pTran.NEW_SUB_PROCESS = rdr["NEW_SUB_PROCESS"].ToString();

                    pTran.PROCESS_DETAIL = rdr["Check_list"].ToString();
                    pTran.NEW_PROCESS_DETAIL = rdr["NEW_Check_list"].ToString();

                    pTran.VIOLATION = rdr["voilation"].ToString();
                    pTran.NEW_VIOLATION = rdr["NEW_voilation"].ToString();

                    pTran.FUNCTIONAL_RESP = rdr["funtional_responible"].ToString();
                    pTran.NEW_FUNCTIONAL_RESP = rdr["NEW_funtional_responible"].ToString();

                    pTran.ROLE_RESP = rdr["Role_responsible"].ToString();
                    pTran.NEW_ROLE_RESP = rdr["NEW_Role_responsible"].ToString();

                    pTran.RISK = rdr["RISK"].ToString();
                    pTran.NEW_RISK = rdr["NEW_RISK"].ToString();

                    pTran.ANNEXURE = rdr["annexure"].ToString();
                    pTran.NEW_ANNEXURE = rdr["NEW_annexure"].ToString();




                    riskTransList.Add(pTran);
                    }
                }
            con.Dispose();
            return riskTransList;
            }

        public List<RiskProcessTransactions> GetRiskProcessTransactions(int procDetailId = 0, int transactionId = 0)
            {
            var con = this.DatabaseConnection();
            List<RiskProcessTransactions> riskTransList = new List<RiskProcessTransactions>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GetRiskProcessTransactions";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("procDetailId", OracleDbType.Int32).Value = procDetailId;
                cmd.Parameters.Add("transactionId", OracleDbType.Int32).Value = transactionId;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    RiskProcessTransactions pTran = new RiskProcessTransactions();
                    pTran.ID = Convert.ToInt32(rdr["ID"]);
                    pTran.PD_ID = Convert.ToInt32(rdr["S_ID"]);
                    pTran.V_ID = Convert.ToInt32(rdr["V_ID"]);
                    if (rdr["ROLE_RESP_ID"].ToString() != null && rdr["ROLE_RESP_ID"].ToString() != "")
                        pTran.DIV_ID = Convert.ToInt32(rdr["ROLE_RESP_ID"]);
                    if (rdr["DIV_NAME"].ToString() != null && rdr["DIV_NAME"].ToString() != "")
                        pTran.DIV_NAME = rdr["DIV_NAME"].ToString();
                    if (rdr["HEADING"].ToString() != null && rdr["HEADING"].ToString() != "")
                        pTran.DESCRIPTION = rdr["HEADING"].ToString();
                    if (rdr["PROCESS_OWNER_ID"].ToString() != null && rdr["PROCESS_OWNER_ID"].ToString() != "")
                        pTran.CONTROL_OWNER = rdr["CONTROL_OWNER"].ToString();
                    pTran.RISK_WEIGHTAGE = Convert.ToInt32(rdr["RISK_ID"]);
                    pTran.RISK = this.GetRiskDescByID(pTran.RISK_WEIGHTAGE);
                    pTran.SUB_PROCESS_NAME = rdr["TITLE"].ToString();
                    pTran.PROCESS_NAME = rdr["P_NAME"].ToString();
                    pTran.VIOLATION_NAME = rdr["V_NAME"].ToString();
                    pTran.PROCESS_STATUS = rdr["STATUS"].ToString();
                    riskTransList.Add(pTran);
                    }
                }
            con.Dispose();
            return riskTransList;
            }

        public List<SubProcessUpdateModelForReviewAndAuthorizeModel> GetUpdatedSubChecklistForReviewAndAuthorize(int statusId)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<SubProcessUpdateModelForReviewAndAuthorizeModel> pmList = new List<SubProcessUpdateModelForReviewAndAuthorizeModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_Get_updated_Sub_Checklist_for_review";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("statusId", OracleDbType.Int32).Value = statusId;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    SubProcessUpdateModelForReviewAndAuthorizeModel pm = new SubProcessUpdateModelForReviewAndAuthorizeModel();
                    pm.PROCESS_NAME = rdr["Process"].ToString();
                    pm.SUB_PROCESS_NAME = rdr["sub_process"].ToString();
                    pm.NEW_PROCESS_NAME = rdr["New_Process"].ToString();
                    pm.COMMENTS = rdr["Comments"].ToString();
                    pm.NEW_SUB_PROCESS_NAME = rdr["new_sub_process"].ToString();
                    pm.P_ID = rdr["t_id"].ToString();
                    pm.NEW_P_ID = rdr["n_t_id"].ToString();
                    pm.SP_ID = rdr["s_id"].ToString();
                    pm.NEW_SP_ID = rdr["n_s_id"].ToString();
                    pmList.Add(pm);

                    }
                }
            con.Dispose();
            return pmList;
            }

        public List<RiskProcessTransactions> GetUpdatedChecklistDetailsForReviewAndAuthorize(int statusId)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<RiskProcessTransactions> riskTransList = new List<RiskProcessTransactions>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_Get_updated_Checklist_for_review";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("statusId", OracleDbType.Int32).Value = statusId;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    RiskProcessTransactions pTran = new RiskProcessTransactions();
                    pTran.ID = Convert.ToInt32(rdr["ID"]);
                    pTran.PD_ID = Convert.ToInt32(rdr["S_ID"]);
                    pTran.V_ID = Convert.ToInt32(rdr["V_ID"]);
                    if (rdr["ROLE_RESP_ID"].ToString() != null && rdr["ROLE_RESP_ID"].ToString() != "")
                        pTran.DIV_ID = Convert.ToInt32(rdr["ROLE_RESP_ID"]);
                    if (rdr["Role_Responsible"].ToString() != null && rdr["Role_Responsible"].ToString() != "")
                        pTran.DIV_NAME = rdr["Role_Responsible"].ToString();
                    if (rdr["HEADING"].ToString() != null && rdr["HEADING"].ToString() != "")
                        pTran.DESCRIPTION = rdr["HEADING"].ToString();
                    if (rdr["PROCESS_OWNER_ID"].ToString() != null && rdr["PROCESS_OWNER_ID"].ToString() != "")
                        pTran.CONTROL_OWNER = rdr["CONTROL_OWNER"].ToString();
                    pTran.RISK_WEIGHTAGE = Convert.ToInt32(rdr["RISK_ID"]);
                    pTran.RISK = rdr["RISK_DESC"].ToString();
                    pTran.SUB_PROCESS_NAME = rdr["TITLE"].ToString();
                    pTran.PROCESS_NAME = rdr["P_NAME"].ToString();
                    pTran.VIOLATION_NAME = rdr["V_NAME"].ToString();
                    pTran.PROCESS_STATUS = rdr["STATUS"].ToString();
                    pTran.PROCESS_COMMENTS = this.GetLatestCommentsOnProcess(pTran.ID);
                    riskTransList.Add(pTran);
                    }
                }
            con.Dispose();
            return riskTransList;
            }

        public RiskProcessDefinition AddRiskProcess(RiskProcessDefinition proc)
            {
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_audit_checklist";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("p_name", OracleDbType.Varchar2).Value = proc.P_NAME;
                cmd.Parameters.Add("RISK_ID", OracleDbType.Int32).Value = proc.RISK_ID;
                cmd.ExecuteReader();
                }
            con.Dispose();
            return proc;
            }

        public RiskProcessDetails AddRiskSubProcess(RiskProcessDetails subProc)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_audit_checklist_sub";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("p_ID", OracleDbType.Int32).Value = subProc.P_ID;
                cmd.Parameters.Add("TITLE", OracleDbType.Varchar2).Value = subProc.TITLE;
                cmd.Parameters.Add("ENTITY_TYPE", OracleDbType.Varchar2).Value = subProc.ENTITY_TYPE;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.ExecuteReader();
                }
            con.Dispose();
            return subProc;
            }

        public RiskProcessTransactions AddRiskSubProcessTransaction(RiskProcessTransactions trans)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_audit_checklist_detail";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("p_id", OracleDbType.Int32).Value = trans.PD_ID;
                cmd.Parameters.Add("DESCRIPTION", OracleDbType.Varchar2).Value = trans.DESCRIPTION;
                cmd.Parameters.Add("V_ID", OracleDbType.Varchar2).Value = trans.V_ID;
                cmd.Parameters.Add("CONTROL_OWNER", OracleDbType.Varchar2).Value = trans.CONTROL_OWNER;
                cmd.Parameters.Add("RISK_WEIGHTAGE", OracleDbType.Varchar2).Value = trans.RISK_WEIGHTAGE;
                cmd.Parameters.Add("ACTION", OracleDbType.Varchar2).Value = trans.ACTION;
                cmd.Parameters.Add("PPNumber", OracleDbType.Varchar2).Value = loggedInUser.PPNumber;
                cmd.ExecuteReader();
                }
            con.Dispose();
            return trans;
            }

        public string AuthorizeSubProcessByAuthorizer(int T_ID, string COMMENTS)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_Approved_Sub_Process_By_Authorizer";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("SID", OracleDbType.Int32).Value = T_ID;
                cmd.Parameters.Add("COMMENTS", OracleDbType.Varchar2).Value = COMMENTS;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public string RefferedBackSubProcessByAuthorizer(int T_ID, string COMMENTS)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            string resp = "";
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_RefferedBack_Sub_checklist_By_Reviewer";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("SID", OracleDbType.Int32).Value = T_ID;
                cmd.Parameters.Add("COMMENTS", OracleDbType.Varchar2).Value = COMMENTS;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public string RecommendProcessTransactionByReviewer(int T_ID, string COMMENTS, int PROCESS_DETAIL_ID = 0, int SUB_PROCESS_ID = 0, string HEADING = "", int V_ID = 0, int CONTROL_ID = 0, int ROLE_ID = 0, int RISK_ID = 0, string ANNEX_CODE = "")
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_Recommend_Checklist_By_Reviewer";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("DID", OracleDbType.Int32).Value = PROCESS_DETAIL_ID;
                cmd.Parameters.Add("SID", OracleDbType.Int32).Value = SUB_PROCESS_ID;
                cmd.Parameters.Add("DESCRIPTION", OracleDbType.Varchar2).Value = HEADING;
                cmd.Parameters.Add("VID", OracleDbType.Int32).Value = V_ID;
                cmd.Parameters.Add("CONTROL_OWNER", OracleDbType.Int32).Value = CONTROL_ID;
                cmd.Parameters.Add("ROLE", OracleDbType.Int32).Value = ROLE_ID;
                cmd.Parameters.Add("RISK", OracleDbType.Int32).Value = RISK_ID;
                cmd.Parameters.Add("ANNEXURE", OracleDbType.Varchar2).Value = ANNEX_CODE;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_ID", OracleDbType.Int32).Value = T_ID;
                cmd.Parameters.Add("COMMENTS", OracleDbType.Varchar2).Value = COMMENTS;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public string RefferedBackProcessTransactionByReviewer(int T_ID, string COMMENTS)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_RefferedBack_checklist_By_Reviewer";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_ID", OracleDbType.Int32).Value = T_ID;
                cmd.Parameters.Add("COMMENTS", OracleDbType.Varchar2).Value = COMMENTS;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public string AuthorizeProcessTransactionByAuthorizer(int T_ID, string COMMENTS)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            string resp = "";
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_approve_checklist_By_Authorizer";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_ID", OracleDbType.Int32).Value = T_ID;
                cmd.Parameters.Add("COMMENTS", OracleDbType.Varchar2).Value = COMMENTS;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public string RefferedBackProcessTransactionByAuthorizer(int T_ID, string COMMENTS)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_RefferedBack_checklist_By_Authorizer";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_ID", OracleDbType.Int32).Value = T_ID;
                cmd.Parameters.Add("COMMENTS", OracleDbType.Varchar2).Value = COMMENTS;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public List<RiskModel> GetRisks()
            {
            var con = this.DatabaseConnection();
            List<RiskModel> riskList = new List<RiskModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GetRisks";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    RiskModel risk = new RiskModel();
                    if (rdr["R_ID"].ToString() != null && rdr["R_ID"].ToString() != "")
                        risk.R_ID = Convert.ToInt32(rdr["R_ID"]);

                    risk.DESCRIPTION = rdr["DESCRIPTION"].ToString();
                    if (risk.R_ID > 0)
                        riskList.Add(risk);
                    }
                }
            con.Dispose();
            return riskList;
            }

        public List<AuditChecklistModel> GetAuthorizeMergeSubChecklist()
            {
            var con = this.DatabaseConnection();
            List<AuditChecklistModel> list = new List<AuditChecklistModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {

                cmd.CommandText = "pkg_ad.P_GET_SUBCHECKILIST";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditChecklistModel chk = new AuditChecklistModel();
                    chk.T_ID = Convert.ToInt32(rdr["S_ID"]);
                    chk.HEADING = rdr["HEADING"].ToString();
                    chk.RISK_SEQUENCE = rdr["RISK_SEQUENCE"].ToString();
                    chk.RISK_WEIGHTAGE = rdr["WEIGHT_ASSIGNED"].ToString();
                    chk.ENTITY_TYPE = Convert.ToInt32(rdr["ENTITY_TYPE"]);
                    // chk.ENTITY_TYPE_NAME = rdr["ENTITY_TYPE_NAME"].ToString();
                    chk.STATUS = rdr["STATUS"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<AuditChecklistModel> GetAnnexureProcess()
            {
            var con = this.DatabaseConnection();
            List<AuditChecklistModel> list = new List<AuditChecklistModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {

                cmd.CommandText = "pkg_ad.p_get_annexure_process";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditChecklistModel chk = new AuditChecklistModel();
                    chk.T_ID = Convert.ToInt32(rdr["ID"]);
                    chk.HEADING = rdr["HEADING"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public string GetLatestCommentsOnProcess(int procId = 0)
            {
            var con = this.DatabaseConnection();
            string response = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GetLatestCommentsOnProcess";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("procId", OracleDbType.Int32).Value = procId;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    response = rdr["comments"].ToString();
                    }
                }
            con.Dispose();
            return response;
            }

        public List<SubCheckListStatus> GetAuditSubChecklist(int PROCESS_ID = 0)
            {
            List<SubCheckListStatus> list = new List<SubCheckListStatus>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_GET_SUB_CHECKLIST_MAKER";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("processid", OracleDbType.Int32).Value = PROCESS_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    SubCheckListStatus chk = new SubCheckListStatus();
                    if (rdr["S_ID"].ToString() != null && rdr["S_ID"].ToString() != "")
                        chk.S_ID = Convert.ToInt32(rdr["S_ID"].ToString());
                    chk.T_ID = Convert.ToInt32(rdr["T_ID"].ToString());
                    chk.PROCESS = rdr["PROCESS"].ToString();
                    chk.HEADING = rdr["SUB_PROCESS"].ToString();
                    chk.RISK_SEQUENCE = rdr["RISK_SEQUENCE"].ToString();
                    chk.RISK_WEIGHTAGE = rdr["WEIGHT_ASSIGNED"].ToString();
                    chk.COMMENTS = rdr["COMMENTS"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public string AddAuditChecklist(string HEADING = "", int ENTITY_TYPE_ID = 0, string RISK_SEQUENCE = "", string RISK_WEIGHTAGE = "")
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_audit_checklist";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("p_name", OracleDbType.Varchar2).Value = HEADING;
                cmd.Parameters.Add("c_seq", OracleDbType.Int32).Value = RISK_SEQUENCE;
                cmd.Parameters.Add("c_weight", OracleDbType.Varchar2).Value = RISK_WEIGHTAGE;
                cmd.Parameters.Add("RISK_ID", OracleDbType.Int32).Value = ENTITY_TYPE_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public string UpdateAuditChecklist(int PROCESS_ID = 0, string HEADING = "", string ACTIVE = "", string RISK_SEQUENCE = "", string RISK_WEIGHTAGE = "")
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_audit_checklist_update";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("t_id", OracleDbType.Int32).Value = PROCESS_ID;
                cmd.Parameters.Add("p_name", OracleDbType.Varchar2).Value = HEADING;
                cmd.Parameters.Add("active", OracleDbType.Varchar2).Value = ACTIVE;
                cmd.Parameters.Add("c_seq", OracleDbType.Int32).Value = RISK_SEQUENCE;
                cmd.Parameters.Add("c_weight", OracleDbType.Varchar2).Value = RISK_WEIGHTAGE;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public string AddAuditSubChecklist(int PROCESS_ID = 0, int ENTITY_TYPE_ID = 0, string HEADING = "", string RISK_SEQUENCE = "", string RISK_WEIGHTAGE = "")
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_audit_checklist_sub";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_ID", OracleDbType.Int32).Value = PROCESS_ID;
                cmd.Parameters.Add("TITLE", OracleDbType.Varchar2).Value = HEADING;
                cmd.Parameters.Add("c_seq", OracleDbType.Int32).Value = RISK_SEQUENCE;
                cmd.Parameters.Add("c_weight", OracleDbType.Varchar2).Value = RISK_WEIGHTAGE;
                cmd.Parameters.Add("ENTITY_TYPE", OracleDbType.Varchar2).Value = ENTITY_TYPE_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public string UpdateAuditSubChecklist(int PROCESS_ID = 0, int OLD_PROCESS_ID = 0, int SUB_PROCESS_ID = 0, string HEADING = "", int ENTITY_TYPE_ID = 0, string RISK_SEQUENCE = "", string RISK_WEIGHTAGE = "")
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_audit_checklist_sub_update";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("TID", OracleDbType.Int32).Value = OLD_PROCESS_ID;
                cmd.Parameters.Add("N_TID", OracleDbType.Int32).Value = PROCESS_ID;
                cmd.Parameters.Add("SID", OracleDbType.Int32).Value = SUB_PROCESS_ID;
                cmd.Parameters.Add("TITLE", OracleDbType.Varchar2).Value = HEADING;
                cmd.Parameters.Add("c_seq", OracleDbType.Int32).Value = RISK_SEQUENCE;
                cmd.Parameters.Add("c_weight", OracleDbType.Varchar2).Value = RISK_WEIGHTAGE;
                cmd.Parameters.Add("ENTITY_TYPE", OracleDbType.Int32).Value = ENTITY_TYPE_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public List<AuditChecklistDetailsModel> GetAuditChecklistDetail(int SUB_PROCESS_ID = 0)
            {
            List<AuditChecklistDetailsModel> list = new List<AuditChecklistDetailsModel>();
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_GetChecklistDetailBySubProcessId";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("subProcessId", OracleDbType.Int32).Value = SUB_PROCESS_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditChecklistDetailsModel chk = new AuditChecklistDetailsModel();
                    if (rdr["S_ID"].ToString() != null && rdr["ID"].ToString() != "")
                        chk.S_ID = Convert.ToInt32(rdr["ID"].ToString());
                    chk.ID = Convert.ToInt32(rdr["S_ID"].ToString());
                    chk.HEADING = rdr["HEADING"].ToString();
                    chk.V_ID = Convert.ToInt32(rdr["V_ID"].ToString());
                    chk.ROLE_RESP_ID = Convert.ToInt32(rdr["role_resp_id"].ToString());
                    chk.PROCESS_OWNER_ID = Convert.ToInt32(rdr["owner_entity_id"].ToString());
                    chk.RISK_ID = Convert.ToInt32(rdr["RISK_ID"].ToString());
                    chk.ANNEX_ID = Convert.ToInt32(rdr["ANNEX"].ToString());
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<AuditChecklistDetailsModel> GetAuditChecklistDetailForRemoveDuplicate(int SUB_PROCESS_ID = 0)
            {
            List<AuditChecklistDetailsModel> list = new List<AuditChecklistDetailsModel>();
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_Get_ChecklistDetail_FOR_DUPLICATE";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("subProcessId", OracleDbType.Int32).Value = SUB_PROCESS_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditChecklistDetailsModel chk = new AuditChecklistDetailsModel();
                    if (rdr["S_ID"].ToString() != null && rdr["ID"].ToString() != "")
                        chk.S_ID = Convert.ToInt32(rdr["ID"].ToString());
                    chk.ID = Convert.ToInt32(rdr["S_ID"].ToString());
                    chk.HEADING = rdr["HEADING"].ToString();
                    chk.V_ID = Convert.ToInt32(rdr["V_ID"].ToString());
                    chk.ROLE_RESP_ID = Convert.ToInt32(rdr["role_resp_id"].ToString());
                    chk.PROCESS_OWNER_ID = Convert.ToInt32(rdr["owner_entity_id"].ToString());
                    chk.RISK_ID = Convert.ToInt32(rdr["RISK_ID"].ToString());
                    chk.ANNEX_ID = Convert.ToInt32(rdr["ANNEX"].ToString());
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<AuditChecklistDetailsModel> GetChecklistDetailForSubProcess(int SUB_PROCESS_ID = 0)
            {
            List<AuditChecklistDetailsModel> list = new List<AuditChecklistDetailsModel>();
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_get_checklistdetail_for_subchecklist";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("sid", OracleDbType.Int32).Value = SUB_PROCESS_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditChecklistDetailsModel chk = new AuditChecklistDetailsModel();
                    chk.HEADING = rdr["details"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<AuditChecklistDetailsModel> GetReferredBackAuditChecklistDetail()
            {
            List<AuditChecklistDetailsModel> list = new List<AuditChecklistDetailsModel>();
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_GetChecklistDetail_ref";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditChecklistDetailsModel chk = new AuditChecklistDetailsModel();
                    if (rdr["S_ID"].ToString() != null && rdr["ID"].ToString() != "")
                        chk.S_ID = Convert.ToInt32(rdr["ID"].ToString());
                    chk.ID = Convert.ToInt32(rdr["S_ID"].ToString());
                    chk.P_ID = Convert.ToInt32(rdr["P_ID"].ToString());
                    chk.HEADING = rdr["HEADING"].ToString();
                    chk.V_ID = Convert.ToInt32(rdr["V_ID"].ToString());
                    chk.COMMENTS = rdr["COMMENTS"].ToString();
                    chk.ROLE_RESP_ID = Convert.ToInt32(rdr["role_resp_id"].ToString());
                    chk.PROCESS_OWNER_ID = Convert.ToInt32(rdr["owner_entity_id"].ToString());
                    chk.RISK_ID = Convert.ToInt32(rdr["RISK_ID"].ToString());
                    chk.ANNEX_ID = Convert.ToInt32(rdr["ANNEX"].ToString());
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public string AddAuditChecklistDetail(int PROCESS_ID = 0, int SUB_PROCESS_ID = 0, string HEADING = "", int V_ID = 0, int CONTROL_ID = 0, int ROLE_ID = 0, int RISK_ID = 0, string ANNEX_CODE = "")
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_audit_checklist_detail";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_ID", OracleDbType.Int32).Value = PROCESS_ID;
                cmd.Parameters.Add("SID", OracleDbType.Int32).Value = SUB_PROCESS_ID;
                cmd.Parameters.Add("DESCRIPTION", OracleDbType.Varchar2).Value = HEADING;
                cmd.Parameters.Add("VID", OracleDbType.Int32).Value = V_ID;
                cmd.Parameters.Add("CONTROL_OWNER", OracleDbType.Int32).Value = CONTROL_ID;
                cmd.Parameters.Add("ROLE", OracleDbType.Int32).Value = ROLE_ID;
                cmd.Parameters.Add("RISK", OracleDbType.Int32).Value = RISK_ID;
                cmd.Parameters.Add("ANNEXURE", OracleDbType.Int32).Value = ANNEX_CODE;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public string UpdateAuditChecklistDetail(int PROCESS_DETAIL_ID = 0, int PROCESS_ID = 0, int SUB_PROCESS_ID = 0, string HEADING = "", int V_ID = 0, int CONTROL_ID = 0, int ROLE_ID = 0, int RISK_ID = 0, string ANNEX_CODE = "")
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.audit_checklist_detail_update";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("DID", OracleDbType.Int32).Value = PROCESS_DETAIL_ID;
                cmd.Parameters.Add("SID", OracleDbType.Int32).Value = SUB_PROCESS_ID;
                cmd.Parameters.Add("DESCRIPTION", OracleDbType.Varchar2).Value = HEADING;
                cmd.Parameters.Add("VID", OracleDbType.Int32).Value = V_ID;
                cmd.Parameters.Add("CONTROL_OWNER", OracleDbType.Int32).Value = CONTROL_ID;
                cmd.Parameters.Add("ROLE", OracleDbType.Int32).Value = ROLE_ID;
                cmd.Parameters.Add("RISK", OracleDbType.Int32).Value = RISK_ID;
                cmd.Parameters.Add("ANNEXURE", OracleDbType.Varchar2).Value = ANNEX_CODE;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public List<UserRelationshipModel> Getchildposting(int e_r_id = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            if (e_r_id == 0)
                e_r_id = Convert.ToInt32(loggedInUser.UserEntityID);

            List<UserRelationshipModel> entitiesList = new List<UserRelationshipModel>();
            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_Getchildposting";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("erid", OracleDbType.Int32).Value = e_r_id;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    UserRelationshipModel entity = new UserRelationshipModel();
                    entity.ENTITY_ID = Convert.ToInt32(rdr["ENTITY_ID"]);
                    entity.C_NAME = rdr["C_NAME"].ToString();
                    entity.C_TYPE_ID = rdr["TYPEID"].ToString();
                    entity.COMPLICE_BY = rdr["COMPLICE_BY"].ToString();
                    entity.AUDIT_BY = rdr["AUDIT_BY"].ToString();
                    entity.GM_OFFICE = rdr["GM_OFFICE"].ToString();
                    entity.REPORTING = rdr["REPORTING"].ToString();
                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }

        public List<UserRelationshipModel> Getparentrepoffice(int r_id = 0)
            {

            List<UserRelationshipModel> entitiesList = new List<UserRelationshipModel>();
            var con = this.DatabaseConnection();
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_Getparentrepoffice";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("rid", OracleDbType.Int32).Value = r_id;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserGroupID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    UserRelationshipModel entity = new UserRelationshipModel();
                    entity.ENTITY_REALTION_ID = Convert.ToInt32(rdr["ENTITY_REALTION_ID"]);
                    entity.ENTITY_ID = Convert.ToInt32(rdr["ENTITY_ID"]);
                    entity.ACTIVE = rdr["ACTIVE"].ToString();
                    entity.DESCRIPTION = rdr["DESCRIPTION"].ToString();
                    entity.ENTITYTYPEDESC = rdr["ENTITYTYPEDESC"].ToString();
                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }

        public List<AuditeeEntitiesModel> GetEntityTypeList()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<AuditeeEntitiesModel> entitiesList = new List<AuditeeEntitiesModel>();
            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GetAuditeeEntityTypes";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditeeEntitiesModel entity = new AuditeeEntitiesModel();
                    entity.NAME = rdr["ENTITY_TYPE"].ToString();
                    entity.CODE = Convert.ToInt32(rdr["entitycode"].ToString());
                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }

        public List<AuditeeEntitiesModel> GetComplianceUnits()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<AuditeeEntitiesModel> entitiesList = new List<AuditeeEntitiesModel>();
            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GET_COMPLIANCE_OFFICE";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditeeEntitiesModel entity = new AuditeeEntitiesModel();
                    entity.NAME = rdr["NAME"].ToString();
                    entity.COM_BY = rdr["ENTITY_ID"].ToString();
                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }

        public List<AuditeeEntitiesModel> GetComplianceOfficer()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<AuditeeEntitiesModel> entitiesList = new List<AuditeeEntitiesModel>();
            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GET_COMPLIANCE_OFFICE";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditeeEntitiesModel entity = new AuditeeEntitiesModel();
                    entity.NAME = rdr["NAME"].ToString();
                    entity.CODE = Convert.ToInt32(rdr["ENTITY_ID"].ToString());
                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }

        public List<UserRelationshipModel> Getrealtionshiptype(int page_id)
            {

            List<UserRelationshipModel> entitiesList = new List<UserRelationshipModel>();
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_Getrealtionshiptype";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserGroupID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("PAGE_ID", OracleDbType.Int32).Value = page_id;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    UserRelationshipModel entity = new UserRelationshipModel();
                    entity.ENTITY_REALTION_ID = Convert.ToInt32(rdr["ENTITY_REALTION_ID"]);
                    entity.FIELD_NAME = rdr["FIELD_NAME"].ToString();
                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }

        public List<ObservationReversalModel> GetAuditeeEngagements(int ENTITY_ID = 0, int PERIOD = 0)
            {
            List<ObservationReversalModel> resp = new List<ObservationReversalModel>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_get_auditee_engagement";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ent_id", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("period", OracleDbType.Int32).Value = PERIOD;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ObservationReversalModel os = new ObservationReversalModel();
                    os.ENG_ID = rdr["ENG_ID"].ToString();
                    os.STATUS = rdr["Eng_name"].ToString();
                    resp.Add(os);
                    }
                }
            con.Dispose();
            return resp;
            }

        public List<ObservationReversalModel> GetEngagementDetailsForStatusReversal(int ENTITY_ID = 0)
            {
            List<ObservationReversalModel> resp = new List<ObservationReversalModel>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_get_audit_engagement";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ent_id", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ObservationReversalModel os = new ObservationReversalModel();
                    os.PLAN_ID = rdr["plan_id"].ToString();
                    os.ENG_ID = rdr["ENG_ID"].ToString();
                    os.TEAM_NAME = rdr["TEAM_NAME"].ToString();
                    os.AUDIT_START_DATE = rdr["AUDIT_STARTDATE"].ToString();
                    os.AUDIT_END_DATE = rdr["AUDIT_ENDDATE"].ToString();
                    os.OP_START_DATE = rdr["OP_STARTDATE"].ToString();
                    os.OP_END_DATE = rdr["OP_ENDDATE"].ToString();
                    os.ENTITY_ID = rdr["ENTITY_ID"].ToString();
                    os.AUDITED_BY_ID = rdr["Auditby_Id"].ToString();
                    os.STATUS_ID = rdr["STATUS_ID"].ToString();
                    os.STATUS = rdr["STATUS"].ToString();
                    resp.Add(os);
                    }
                }
            con.Dispose();
            return resp;
            }

        public List<EngagementObservationsForStatusReversalModel> GetObservationDetailsForStatusReversal(int ENG_ID = 0)
            {
            List<EngagementObservationsForStatusReversalModel> resp = new List<EngagementObservationsForStatusReversalModel>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_get_audit_observtion";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    EngagementObservationsForStatusReversalModel os = new EngagementObservationsForStatusReversalModel();
                    os.ID = rdr["ID"].ToString();
                    os.MEMO_NO = rdr["MEMO_NO"].ToString();
                    os.GIST = rdr["GIST"].ToString();
                    os.MEMO_DATE = rdr["MEMO_DATE"].ToString();
                    os.HEADING = rdr["HEADINGS"].ToString();
                    os.RISK = rdr["RISK"].ToString();
                    os.STATUS = rdr["STATUS"].ToString();
                    resp.Add(os);
                    }
                }
            con.Dispose();
            return resp;
            }

        public List<ObservationStatusReversalModel> GetObservationReversalStatus()
            {

            List<ObservationStatusReversalModel> stList = new List<ObservationStatusReversalModel>();
            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_get_audit_observtion_status";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ObservationStatusReversalModel st = new ObservationStatusReversalModel();
                    st.STATUS_NAME = rdr["statusname"].ToString();
                    st.STATUS_ID = Convert.ToInt32(rdr["statusid"].ToString());
                    stList.Add(st);
                    }
                }
            con.Dispose();
            return stList;

            }

        public List<ObservationStatusReversalModel> GetEngagementReversalStatus(int ENG_ID)
            {

            List<ObservationStatusReversalModel> stList = new List<ObservationStatusReversalModel>();
            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_get_audit_engagement_status";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ObservationStatusReversalModel st = new ObservationStatusReversalModel();
                    st.STATUS_NAME = rdr["status"].ToString();
                    st.STATUS_ID = Convert.ToInt32(rdr["id"].ToString());
                    stList.Add(st);
                    }
                }
            con.Dispose();
            return stList;

            }

        public List<AuditeeRiskModel> GetAuditeeRisk(int ENG_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<AuditeeRiskModel> pdetails = new List<AuditeeRiskModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GetAuditeeRisk";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditeeRiskModel zb = new AuditeeRiskModel();
                    zb.MAX_NUMBER = rdr["max_number"].ToString();
                    zb.RISK_AREAS = rdr["risk_areas"].ToString();
                    zb.MARKS = rdr["Marks"].ToString();
                    pdetails.Add(zb);
                    }
                }
            con.Dispose();
            return pdetails;
            }

        public List<RiskAssessmentEntTypeModel> GetAuditeeRiskForEntTypes(int ENT_TYPE_ID = 0, int PERIOD = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<RiskAssessmentEntTypeModel> pdetails = new List<RiskAssessmentEntTypeModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_Get_Entity_Risk";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_TYP", OracleDbType.Int32).Value = ENT_TYPE_ID;
                cmd.Parameters.Add("Period", OracleDbType.Int32).Value = PERIOD;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    RiskAssessmentEntTypeModel zb = new RiskAssessmentEntTypeModel();
                    zb.RISK_CATEGORY = rdr["risk_category"].ToString();
                    zb.RISK_RATING = rdr["risk_rating"].ToString();
                    zb.NAME = rdr["name"].ToString();
                    zb.PARENT_OFFICE = rdr["parent_office"].ToString();
                    zb.BRANCH_CODE = rdr["branch_code"].ToString();

                    pdetails.Add(zb);
                    }
                }
            con.Dispose();
            return pdetails;
            }

        public List<AuditeeRiskModeldetails> GetAuditeeRiskDetails(int ENG_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<AuditeeRiskModeldetails> pdetails = new List<AuditeeRiskModeldetails>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GetAuditeeRisk_details";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditeeRiskModeldetails zb = new AuditeeRiskModeldetails();
                    zb.MAX_NUMBER = rdr["max_number"].ToString();
                    zb.RISK_AREAS = rdr["risk_areas"].ToString();
                    zb.RISK_MARKS = rdr["risk_based_marks"].ToString();
                    zb.NO_OBS = rdr["number_of_observations"].ToString();
                    zb.AVG_MARKS = rdr["weighted_average_marks"].ToString();
                    zb.W_AVG = rdr["weightage_average"].ToString();
                    zb.G_RISK = rdr["gravity_risk"].ToString();
                    pdetails.Add(zb);
                    }
                }
            con.Dispose();
            return pdetails;
            }

        public string MergeDuplicateProcesses(string PROCESS_ID, string M_PROC_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_merge_checklist";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("CID", OracleDbType.Varchar2).Value = PROCESS_ID;
                cmd.Parameters.Add("mcid", OracleDbType.Varchar2).Value = M_PROC_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["REMARKS"].ToString();

                    }

                }
            con.Dispose();
            return resp;
            }

        public string MergeDuplicateSubProcesses(string PROCESS_ID, string SUB_PROCESS_ID, string M_SUB_PROC_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_merge_sub_checklist";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("SID", OracleDbType.Varchar2).Value = SUB_PROCESS_ID;
                cmd.Parameters.Add("msid", OracleDbType.Varchar2).Value = M_SUB_PROC_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["REMARKS"].ToString();

                    }

                }
            con.Dispose();
            return resp;
            }

        public bool MergeDuplicateChecklists(string CHECKLIST_ID, string M_CHECKLIST_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_REMOVE_DUPLICATE_CHECKLIST_DETAILS";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("C_ID", OracleDbType.Varchar2).Value = CHECKLIST_ID;
                cmd.Parameters.Add("D_ID", OracleDbType.Varchar2).Value = M_CHECKLIST_ID;
                cmd.ExecuteReader();

                }
            con.Dispose();
            return true;
            }

        public List<AuditChecklistModel> GetAuditProcessListForMergeDuplicate()
            {
            var con = this.DatabaseConnection();
            List<AuditChecklistModel> list = new List<AuditChecklistModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {

                cmd.CommandText = "pkg_ad.P_GET_DUPLICATE_CHECKLIST_DETAILS_DROPDOWN";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditChecklistModel chk = new AuditChecklistModel();
                    chk.T_ID = Convert.ToInt32(rdr["ID"]);
                    chk.HEADING = rdr["Main_checklist"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<MergeDuplicateProcessModel> GetDuplicateProcesses(int PROCESS_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<MergeDuplicateProcessModel> list = new List<MergeDuplicateProcessModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {

                cmd.CommandText = "pkg_ad.p_Get_Checklist_MERGER_FOR_REVIEW";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("CID", OracleDbType.Int32).Value = PROCESS_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Varchar2).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Varchar2).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Varchar2).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    MergeDuplicateProcessModel chk = new MergeDuplicateProcessModel();
                    chk.ID = Convert.ToInt32(rdr["cid"]);
                    chk.M_ID = Convert.ToInt32(rdr["m_cid"]);
                    chk.HEADING = rdr["for_merger"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<MergeDuplicateProcessModel> GetDuplicateSubProcesses(int SUB_PROCESS_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<MergeDuplicateProcessModel> list = new List<MergeDuplicateProcessModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {

                cmd.CommandText = "pkg_ad.p_Get_sub_Checklist_MERGER_FOR_REVIEW";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("SID", OracleDbType.Int32).Value = SUB_PROCESS_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Varchar2).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Varchar2).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Varchar2).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    MergeDuplicateProcessModel chk = new MergeDuplicateProcessModel();
                    chk.ID = Convert.ToInt32(rdr["sid"]);
                    chk.M_ID = Convert.ToInt32(rdr["m_sid"]);
                    chk.HEADING = rdr["for_merger"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<MergeDuplicateChecklistModel> GetDuplicateChecklists(int PROCESS_ID)
            {
            var con = this.DatabaseConnection();
            List<MergeDuplicateChecklistModel> list = new List<MergeDuplicateChecklistModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {

                cmd.CommandText = "pkg_ad.P_GET_DUPLICATE_CHECKLIST_DETAILS";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("D_ID", OracleDbType.Int32).Value = PROCESS_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    MergeDuplicateChecklistModel chk = new MergeDuplicateChecklistModel();
                    chk.ID = Convert.ToInt32(rdr["C_ID"]);
                    chk.HEADING = rdr["HEADING"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public MergeDuplicateChecklistModel GetDuplicateChecklistsCount(int PROCESS_ID)
            {
            MergeDuplicateChecklistModel chk = new MergeDuplicateChecklistModel();

            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {

                cmd.CommandText = "pkg_ad.P_GET_DUPLICATE_CHECKLIST_DETAILS_COUNT";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("D_ID", OracleDbType.Int32).Value = PROCESS_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    chk.NEW_COUNT = rdr["New"].ToString();
                    chk.OLD_COUNT = rdr["Old"].ToString();
                    }
                }
            con.Dispose();
            return chk;
            }

        public string AuthorizeMergeDuplicateProcesses(int PROCESS_ID, int AUTH_P_ID)
            {
            string resp = "";
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {

                cmd.CommandText = "pkg_ad.P_AUTHORIZE_MERGER_CHECKLIST";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("C_ID", OracleDbType.Int32).Value = PROCESS_ID;
                cmd.Parameters.Add("M_CID", OracleDbType.Int32).Value = AUTH_P_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();

                    }
                }
            con.Dispose();
            return resp;
            }

        public string AuthorizeMergeDuplicateSubProcesses(int SUB_PROCESS_ID, int AUTH_S_P_ID)
            {
            string resp = "";
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {

                cmd.CommandText = "pkg_ad.P_AUTHORIZE_MERGER_CHECKLIST_SUB";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("SID", OracleDbType.Int32).Value = SUB_PROCESS_ID;
                cmd.Parameters.Add("M_SID", OracleDbType.Int32).Value = AUTH_S_P_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();

                    }
                }
            con.Dispose();
            return resp;
            }

        public string AuthorizeMergeDuplicateChecklists(int PROCESS_ID)
            {
            string resp = "";
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {

                cmd.CommandText = "pkg_ad.P_AUTHORIZE_DUPLICATE_CHECKLIST_DETAILS";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("D_ID", OracleDbType.Int32).Value = PROCESS_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();

                    }
                }
            con.Dispose();
            return resp;
            }

        public string UpdateObservationStatusForReversal(int OBS_ID, int NEW_STATUS_ID, int ENG_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            var resp = "";

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_audit_observation_reversal";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Varchar2).Value = ENG_ID;
                cmd.Parameters.Add("OBS_ID", OracleDbType.Varchar2).Value = OBS_ID;
                cmd.Parameters.Add("S_ID", OracleDbType.Varchar2).Value = NEW_STATUS_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public List<AdminNewUsersAIS> AdminNewUsersInAIS()
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AdminNewUsersAIS> resp = new List<AdminNewUsersAIS>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_get_new_user";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AdminNewUsersAIS rd = new AdminNewUsersAIS();
                    rd.ENTITY_NAME = rdr["name"].ToString();
                    rd.ENTITY_ID = rdr["entity_id"].ToString();
                    rd.DESIGNATION = rdr["designation"].ToString();
                    rd.DESIGNATION_CODE = rdr["designationcode"].ToString();
                    rd.EMPLOYEE_TYPE = rdr["employeetype"].ToString();
                    rd.POSTING_TYPE = rdr["posting_Type"].ToString();
                    rd.PPNO = rdr["ppno"].ToString();
                    rd.EMP_NAME = rdr["e_name"].ToString();
                    rd.CODE = rdr["code"].ToString();
                    resp.Add(rd);

                    }
                }
            con.Dispose();
            return resp;

            }

        public List<HREntitiesModel> GetHREntitiesForAdminPanelEntityAddition(string ENTITY_NAME, string ENTITY_CODE)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<HREntitiesModel> resp = new List<HREntitiesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GET_HR_ENTITIES";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_CODE", OracleDbType.Int32).Value = ENTITY_CODE;
                cmd.Parameters.Add("ENT_NAME", OracleDbType.Varchar2).Value = ENTITY_NAME;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    HREntitiesModel mod = new HREntitiesModel();
                    mod.REPORTING_CODE = rdr["Rept_code"].ToString();
                    mod.REPORTING_NAME = rdr["Rept_name"].ToString();
                    mod.REPORTING_STATUS = rdr["Rept_status"].ToString();
                    mod.REPORTING_INDICATOR = rdr["Rept_ind"].ToString();

                    mod.ENTITY_CODE = rdr["Entity_code"].ToString();
                    mod.ENTITY_NAME = rdr["Entity_name"].ToString();
                    mod.ENTITY_STATUS = rdr["Entity_status"].ToString();
                    mod.ENTITY_INDICATOR = rdr["ind"].ToString();
                    resp.Add(mod);

                    }
                }
            con.Dispose();
            return resp;

            }

        public List<AISEntitiesModel> GetAISEntitiesForAdminPanelEntityAddition(string ENTITY_NAME, string ENTITY_CODE, int ENT_TYPE_ID = 0)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AISEntitiesModel> resp = new List<AISEntitiesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GET_AIS_ENTITIES";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_CODE", OracleDbType.Int32).Value = ENTITY_CODE;
                cmd.Parameters.Add("ENT_NAME", OracleDbType.Varchar2).Value = ENTITY_NAME;
                cmd.Parameters.Add("ENT_TYPE", OracleDbType.Int32).Value = ENT_TYPE_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AISEntitiesModel mod = new AISEntitiesModel();
                    mod.ENTITY_ID = rdr["entity_id"].ToString();
                    mod.ENTITY_CODE = rdr["code"].ToString();
                    mod.DESCRIPTION = rdr["description"].ToString();
                    mod.ENTITY_NAME = rdr["name"].ToString();
                    mod.TYPE_ID = rdr["type_id"].ToString();
                    mod.AUDIT_BY_ID = rdr["auditby_id"].ToString();
                    mod.AUDIT_BY = rdr["audit_by"].ToString();
                    mod.STATUS = rdr["active"].ToString();
                    mod.AUDITABLE = rdr["auditable"].ToString();
                    resp.Add(mod);

                    }
                }
            con.Dispose();
            return resp;

            }

        public string UpdateAISEntityForAdminPanelEntityAddition(string ENTITY_ID, string ENTITY_NAME, string ENTITY_CODE, string AUDITABLE, string AUDIT_BY_ID, string ENTITY_TYPE_ID, string ENT_DESC, string STATUS)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_UpdateENTITIEES";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserGroupID;
                cmd.Parameters.Add("e_CODE", OracleDbType.Int32).Value = ENTITY_CODE;
                cmd.Parameters.Add("e_NAME", OracleDbType.Varchar2).Value = ENTITY_NAME;
                cmd.Parameters.Add("e_DISCRIPTION", OracleDbType.Varchar2).Value = ENT_DESC;
                cmd.Parameters.Add("e_AUDITEDBY", OracleDbType.Varchar2).Value = AUDIT_BY_ID;
                cmd.Parameters.Add("e_TYPEID", OracleDbType.Int32).Value = ENTITY_TYPE_ID;
                cmd.Parameters.Add("e_STATUS", OracleDbType.Varchar2).Value = STATUS;
                cmd.Parameters.Add("e_AUDITABLE", OracleDbType.Varchar2).Value = AUDITABLE;
                cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public string AddAISEntityForAdminPanelEntityAddition(string ENTITY_NAME, string ENTITY_CODE, string AUDITABLE, string AUDIT_BY_ID, string ENTITY_TYPE_ID, string ENT_DESC, string STATUS)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_InsertENTITIEES";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserGroupID;
                cmd.Parameters.Add("e_CODE", OracleDbType.Int32).Value = ENTITY_CODE;
                cmd.Parameters.Add("e_NAME", OracleDbType.Varchar2).Value = ENTITY_NAME;
                cmd.Parameters.Add("e_DISCRIPTION", OracleDbType.Varchar2).Value = ENT_DESC;
                cmd.Parameters.Add("e_AUDITEDBY", OracleDbType.Varchar2).Value = AUDIT_BY_ID;
                cmd.Parameters.Add("e_TYPEID", OracleDbType.Int32).Value = ENTITY_TYPE_ID;
                cmd.Parameters.Add("e_STATUS", OracleDbType.Varchar2).Value = STATUS;
                cmd.Parameters.Add("e_AUDITABLE", OracleDbType.Varchar2).Value = AUDITABLE;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public List<EntityMappingForEntityAddition> GetAISEntityMappingForAdminPanelEntityAddition(string ENTITY_ID)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<EntityMappingForEntityAddition> respOut = new List<EntityMappingForEntityAddition>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_get_auditee_entities_mapping";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();

                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("T_ID", OracleDbType.Varchar2).Value = 0;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    EntityMappingForEntityAddition resp = new EntityMappingForEntityAddition();
                    resp.PARENT_ID = rdr["parent_id"].ToString();
                    resp.PARENT_CODE = rdr["parent_code"].ToString();
                    resp.CHILD_CODE = rdr["child_code"].ToString();
                    resp.CHILD_ID = rdr["entity_id"].ToString();
                    resp.AUDITED_BY = rdr["auditedby"].ToString();
                    resp.PARENT_NAME = rdr["p_name"].ToString();
                    resp.CHILD_NAME = rdr["c_name"].ToString();
                    resp.PARENT_TYPE_ID = rdr["p_type_id"].ToString();
                    resp.CHILD_TYPE_ID = rdr["c_type_id"].ToString();
                    resp.RELATION_TYPE_ID = rdr["relation_type_id"].ToString();
                    respOut.Add(resp);
                    }
                }
            con.Dispose();
            return respOut;

            }

        public string UpdateAISEntityMappingForAdminPanelEntityAddition(string P_ENTITY_ID, string ENTITY_ID, string RELATION_TYPE_ID)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_UPDATE_ENTITIES_MAPPING";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_ENT_ID", OracleDbType.Int32).Value = P_ENTITY_ID;
                cmd.Parameters.Add("RELATION_ID", OracleDbType.Int32).Value = RELATION_TYPE_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public string AddAISEntityMappingForAdminPanelEntityAddition(string P_ENTITY_ID, string ENTITY_ID, string RELATION_TYPE_ID)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_ADD_ENTITIES_MAPPING";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_ENT_ID", OracleDbType.Int32).Value = P_ENTITY_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("RELATION_ID", OracleDbType.Int32).Value = RELATION_TYPE_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public string UpdateNewUsersAdminPanel(int PPNO)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_UPDATE_NEW_USER";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("PPNO", OracleDbType.Int32).Value = PPNO;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public List<UserRoleDetailAdminPanelModel> GetUserDetailAdminPanel(string DESINATION_CODE)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<UserRoleDetailAdminPanelModel> resp = new List<UserRoleDetailAdminPanelModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_get_user_role_type";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("D_CODE", OracleDbType.Int32).Value = DESINATION_CODE;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    UserRoleDetailAdminPanelModel m = new UserRoleDetailAdminPanelModel();
                    m.GROUP_NAME = rdr["group_name"].ToString();
                    m.GROUP_ID = rdr["group_id"].ToString();
                    m.ROLE_ID = rdr["role_id"].ToString();
                    resp.Add(m);
                    }
                }
            con.Dispose();
            return resp;

            }

        public List<EntitiesShiftingDetailsModel> GetEntityShiftingDetails(string ENTITY_ID = "")
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<EntitiesShiftingDetailsModel> resp = new List<EntitiesShiftingDetailsModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_Get_details_for_entity_shifting";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    EntitiesShiftingDetailsModel m = new EntitiesShiftingDetailsModel();
                    m.NAME = rdr["NAME"].ToString();
                    m.E_SIZE = rdr["E_SIZE"].ToString();
                    m.RISK = rdr["RISK"].ToString();
                    m.ENG_ID = rdr["ENG_ID"].ToString();
                    m.START_DATE = rdr["START_DATE"].ToString();
                    m.END_DATE = rdr["END_DATE"].ToString();
                    m.TOTAL_PARA = rdr["TOTAL_PARA"].ToString();
                    m.LEGACY_PARA = rdr["LEGAGY_PARA"].ToString();
                    m.LEGACY_OPEN = rdr["LEGACY_OPEN"].ToString();
                    m.LEGACY_CLOSE = rdr["LEGACY_CLOSE"].ToString();
                    m.AIS_PARA = rdr["AIS_PARA"].ToString();
                    m.AIS_OPEN = rdr["AIS_OPEN"].ToString();
                    m.AIS_CLOSE = rdr["AIS_CLOSE"].ToString();
                    m.COMP_SUB = rdr["COMP_SUB"].ToString();
                    resp.Add(m);
                    }
                }
            con.Dispose();
            return resp;

            }

        public List<AuditEntitiesModel> GetEntityTypes()
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditEntitiesModel> resp = new List<AuditEntitiesModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_Get_Entities_types";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditEntitiesModel m = new AuditEntitiesModel();
                    m.AUTID = Convert.ToInt32(rdr["AUTID"].ToString());
                    m.ENTITYCODE = rdr["ENTITYCODE"].ToString();
                    m.ENTITYTYPEDESC = rdr["ENTITYTYPEDESC"].ToString();
                    if (rdr["AUDITABLE"].ToString() == "")
                        m.AUDITABLE = "N";
                    else
                        m.AUDITABLE = rdr["AUDITABLE"].ToString();
                    m.AUDITEDBY = rdr["AUDITEDBY"].ToString();
                    m.AUDITED_BY_ENTITY = rdr["AUDITED_BY_ENITITY"].ToString();
                    m.AUDIT_TYPE = rdr["AUDIT_TYPE"].ToString();
                    resp.Add(m);
                    }
                }
            con.Dispose();
            return resp;

            }

        public string UpdateEntityTypes(AuditEntitiesModel ENTITY_MODEL)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_update_Entities_types";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("aut_id", OracleDbType.Int32).Value = ENTITY_MODEL.AUTID;
                cmd.Parameters.Add("e_code", OracleDbType.Int32).Value = ENTITY_MODEL.ENTITYCODE;
                cmd.Parameters.Add("e_desc", OracleDbType.Varchar2).Value = ENTITY_MODEL.ENTITYTYPEDESC;
                cmd.Parameters.Add("e_auditable", OracleDbType.Varchar2).Value = ENTITY_MODEL.AUDITABLE;
                cmd.Parameters.Add("e_auditby_code", OracleDbType.Int32).Value = ENTITY_MODEL.AUDITEDBY;
                cmd.Parameters.Add("e_auditby_id", OracleDbType.Int32).Value = ENTITY_MODEL.AUDITED_BY_ENTITY;
                cmd.Parameters.Add("e_type", OracleDbType.Varchar2).Value = ENTITY_MODEL.AUDIT_TYPE;
                cmd.Parameters.Add("e_autid", OracleDbType.Int32).Value = ENTITY_MODEL.E_AUTID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public List<AuditEntityRelationsModel> GetEntityRelations()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditEntityRelationsModel> resp = new List<AuditEntityRelationsModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_Get_Entities_Relationship";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditEntityRelationsModel m = new AuditEntityRelationsModel();
                    if (rdr["ID"].ToString() != null && rdr["ID"].ToString() != "")
                        m.ID = Convert.ToInt32(rdr["ID"].ToString());
                    if (rdr["ENTITY_REALTION_ID"].ToString() != null && rdr["ENTITY_REALTION_ID"].ToString() != "")
                        m.ENTITY_REALTION_ID = Convert.ToInt32(rdr["ENTITY_REALTION_ID"].ToString());
                    if (rdr["PARENT_ENTITY_TYPEID"].ToString() != null && rdr["PARENT_ENTITY_TYPEID"].ToString() != "")
                        m.PARENT_ENTITY_TYPEID = Convert.ToInt32(rdr["PARENT_ENTITY_TYPEID"].ToString());
                    if (rdr["CHILD_ENTITY_TYPEID"].ToString() != null && rdr["CHILD_ENTITY_TYPEID"].ToString() != "")
                        m.CHILD_ENTITY_TYPEID = Convert.ToInt32(rdr["CHILD_ENTITY_TYPEID"].ToString());

                    m.STATUS = rdr["STATUS"].ToString();
                    m.PARENT_NAME = rdr["PARENT_NAME"].ToString();
                    m.CHILD_NAME = rdr["chlid_name"].ToString();

                    resp.Add(m);
                    }
                }
            con.Dispose();
            return resp;
            }

        public List<EntitiesMappingModel> GetEntitiesMapping(string ENT_ID, string P_TYPE, string C_TYPE, string RELATION_TYPE, string IND)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<EntitiesMappingModel> resp = new List<EntitiesMappingModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GET_ENTITIES_MAPPING";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ent_id", OracleDbType.Int32).Value = ENT_ID;
                cmd.Parameters.Add("P_TYPE", OracleDbType.Varchar2).Value = P_TYPE;
                cmd.Parameters.Add("C_TYPE", OracleDbType.Varchar2).Value = C_TYPE;
                cmd.Parameters.Add("REALTION_TYPE", OracleDbType.Int32).Value = RELATION_TYPE;
                cmd.Parameters.Add("ind", OracleDbType.Varchar2).Value = IND;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    EntitiesMappingModel m = new EntitiesMappingModel();
                    m.PARENT_ID = rdr["PARENT_ID"].ToString();
                    m.PARENT_CODE = rdr["PARENT_CODE"].ToString();
                    m.CHILD_CODE = rdr["CHILD_CODE"].ToString();
                    m.ENTITY_ID = rdr["ENTITY_ID"].ToString();
                    m.AUDITEDBY = rdr["AUDITEDBY"].ToString();
                    m.STATUS = rdr["STATUS"].ToString();
                    m.P_NAME = rdr["P_NAME"].ToString();
                    m.C_NAME = rdr["C_NAME"].ToString();
                    m.P_TYPE_ID = rdr["P_TYPE_ID"].ToString();
                    m.C_TYPE_ID = rdr["C_TYPE_ID"].ToString();
                    m.RELATION_TYPE_ID = rdr["RELATION_TYPE_ID"].ToString();
                    resp.Add(m);
                    }
                }
            con.Dispose();
            return resp;

            }

        public List<EntitiesMappingModel> GetEntitiesMappingReporting(string ENT_ID, string P_TYPE, string C_TYPE, string RELATION_TYPE, string IND)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<EntitiesMappingModel> resp = new List<EntitiesMappingModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GET_ENTITIES_MAPPING_REPORTING";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ent_id", OracleDbType.Int32).Value = ENT_ID;
                cmd.Parameters.Add("P_TYPE", OracleDbType.Varchar2).Value = P_TYPE;
                cmd.Parameters.Add("C_TYPE", OracleDbType.Varchar2).Value = C_TYPE;
                cmd.Parameters.Add("REALTION_TYPE", OracleDbType.Int32).Value = RELATION_TYPE;
                cmd.Parameters.Add("ind", OracleDbType.Varchar2).Value = IND;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    EntitiesMappingModel m = new EntitiesMappingModel();
                    m.PARENT_ID = rdr["PARENT_ID"].ToString();
                    m.PARENT_CODE = rdr["PARENT_CODE"].ToString();
                    m.CHILD_CODE = rdr["CHILD_CODE"].ToString();
                    m.ENTITY_ID = rdr["ENTITY_ID"].ToString();
                    m.AUDITEDBY = rdr["AUDITEDBY"].ToString();
                    m.STATUS = rdr["STATUS"].ToString();
                    m.P_NAME = rdr["P_NAME"].ToString();
                    m.C_NAME = rdr["C_NAME"].ToString();
                    m.P_TYPE_ID = rdr["P_TYPE_ID"].ToString();
                    m.C_TYPE_ID = rdr["C_TYPE_ID"].ToString();
                    m.RELATION_TYPE_ID = rdr["RELATION_TYPE_ID"].ToString();
                    resp.Add(m);
                    }
                }
            con.Dispose();
            return resp;

            }

        public List<EntitiesMappingModel> GetParentChildEntities(string P_TYPE_ID, string C_TYPE_ID)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<EntitiesMappingModel> resp = new List<EntitiesMappingModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_get_entities";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_TYPE", OracleDbType.Varchar2).Value = P_TYPE_ID;
                cmd.Parameters.Add("C_TYPE", OracleDbType.Varchar2).Value = C_TYPE_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    EntitiesMappingModel m = new EntitiesMappingModel();
                    m.PARENT_ID = rdr["PARENT_ID"].ToString();
                    m.P_NAME = rdr["P_NAME"].ToString();
                    resp.Add(m);
                    }
                }
            con.Dispose();
            return resp;

            }

        public string SubmitEntityShiftingFromAdminPanel(string FROM_ENT_ID, string TO_ENT_ID, string CIR_REF_NO, DateTime CIR_DATE, string CIR)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_Add_Entity_shifting";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("Old_Ent_id", OracleDbType.Varchar2).Value = FROM_ENT_ID;
                cmd.Parameters.Add("new_ent_id", OracleDbType.Varchar2).Value = TO_ENT_ID;
                cmd.Parameters.Add("P_NO", OracleDbType.Varchar2).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Varchar2).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Varchar2).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("CIR_NO", OracleDbType.Varchar2).Value = CIR_REF_NO;
                cmd.Parameters.Add("CIR_ATTACH", OracleDbType.Clob).Value = CIR;
                cmd.Parameters.Add("CIR_DATE", OracleDbType.Date).Value = CIR_DATE;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public string SubmitEntityConvToIslamicFromAdminPanel(string FROM_ENT_ID, string TO_ENT_ID)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_Shift_BR_to_islamic";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("Old_br", OracleDbType.Int32).Value = FROM_ENT_ID;
                cmd.Parameters.Add("new_br", OracleDbType.Int32).Value = TO_ENT_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public List<GroupModel> GetRolesForComplianceFlow()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<GroupModel> groupList = new List<GroupModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_get_roles_for_compliance_flow";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    GroupModel grp = new GroupModel();
                    grp.GROUP_ID = Convert.ToInt32(rdr["GROUP_ID"]);
                    grp.GROUP_NAME = rdr["GROUP_NAME"].ToString();
                    grp.GROUP_DESCRIPTION = rdr["DESCRIPTION"].ToString();
                    grp.GROUP_CODE = Convert.ToInt32(rdr["GROUP_ID"]);
                    grp.ISACTIVE = rdr["STATUS"].ToString();
                    groupList.Add(grp);
                    }
                }
            con.Dispose();
            return groupList;
            }

        public List<GroupModel> GetHRDesignation()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<GroupModel> groupList = new List<GroupModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GetRoleResponsibilities";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                //cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                //cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                //cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    GroupModel grp = new GroupModel();
                    grp.GROUP_ID = Convert.ToInt32(rdr["DESIGNATIONCODE"]);
                    grp.GROUP_CODE = Convert.ToInt32(rdr["DESIGNATIONCODE"]);
                    grp.GROUP_NAME = rdr["DESCRIPTION"].ToString();
                    groupList.Add(grp);
                    }
                }
            con.Dispose();
            return groupList;
            }

        public List<AuditeeEntitiesModel> GetEntityTypesForComplianceFlow()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<AuditeeEntitiesModel> entitiesList = new List<AuditeeEntitiesModel>();
            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_get_ent_types_for_compliance_flow";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserGroupID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditeeEntitiesModel entity = new AuditeeEntitiesModel();
                    entity.NAME = rdr["ENTITY_TYPE"].ToString();
                    entity.CODE = Convert.ToInt32(rdr["entitycode"].ToString());
                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }

        public List<AuditeeEntitiesModel> GetEntityTypesForHRDesignationWiseRole()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<AuditeeEntitiesModel> entitiesList = new List<AuditeeEntitiesModel>();
            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_get_ent_types_for_hr_designation";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserGroupID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditeeEntitiesModel entity = new AuditeeEntitiesModel();
                    entity.NAME = rdr["ENTITY_TYPE"].ToString();
                    entity.CODE = Convert.ToInt32(rdr["entitycode"].ToString());
                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }

        public List<AuditeeEntitiesModel> GetComplianceStatusesForComplianceFlow()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<AuditeeEntitiesModel> entitiesList = new List<AuditeeEntitiesModel>();
            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_get_compliance_statuses_for_compliance_flow";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditeeEntitiesModel entity = new AuditeeEntitiesModel();
                    entity.NAME = rdr["statusname"].ToString();
                    entity.CODE = Convert.ToInt32(rdr["statusid"].ToString());
                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }

        public List<ComplianceFlowModel> GetComplianceFlowByEntityType(int ENTITY_TYPE_ID = 0, int GROUP_ID = 0)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<ComplianceFlowModel> resp = new List<ComplianceFlowModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_get_entity_type_compliance_flow";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("E_TYPE", OracleDbType.Int32).Value = ENTITY_TYPE_ID;
                cmd.Parameters.Add("G_ID", OracleDbType.Int32).Value = GROUP_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ComplianceFlowModel cm = new ComplianceFlowModel();

                    cm.ENTITY_TYPE_ID = rdr["e_id"].ToString();
                    cm.ENTITY_TYPE_NAME = rdr["e_name"].ToString();
                    cm.GROUP_ID = rdr["g_id"].ToString();
                    cm.GROUP_NAME = rdr["g_name"].ToString();
                    cm.PREV_GROUP_ID = rdr["prev_r_id"].ToString() == "" ? "0" : rdr["prev_r_id"].ToString();
                    cm.PREV_GROUP_NAME = rdr["prev_r_name"].ToString();
                    cm.NEXT_GROUP_ID = rdr["next_r_id"].ToString() == "" ? "0" : rdr["next_r_id"].ToString();
                    cm.NEXT_GROUP_NAME = rdr["next_r_name"].ToString();
                    cm.COMP_DOWN_STATUS = rdr["c_status_down"].ToString();
                    cm.COMP_DOWN_STATUS_DESC = rdr["c_status_down_desc"].ToString();
                    cm.COMP_UP_STATUS = rdr["c_status_up"].ToString();
                    cm.COMP_UP_STATUS_DESC = rdr["c_status_up_desc"].ToString();
                    cm.ID = Convert.ToInt32(rdr["id"].ToString());


                    resp.Add(cm);

                    }
                }
            con.Dispose();
            return resp;

            }

        public string AddComplianceFlow(string ID, string ENTITY_TYPE_ID, string GROUP_ID, string PREV_GROUP_ID, string NEXT_GROUP_ID, string COM_UP_STATUS
            , string COM_DOWN_STATUS)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_ADD_UPDATE_COMPLIANCE_FLOW";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("F_ID", OracleDbType.Int32).Value = ID;
                cmd.Parameters.Add("TYPE_ID", OracleDbType.Int32).Value = ENTITY_TYPE_ID;
                cmd.Parameters.Add("GROUP_ID", OracleDbType.Int32).Value = GROUP_ID;
                cmd.Parameters.Add("P_GROUP_ID", OracleDbType.Int32).Value = PREV_GROUP_ID;
                cmd.Parameters.Add("N_GROUP_ID", OracleDbType.Int32).Value = NEXT_GROUP_ID;
                cmd.Parameters.Add("C_UP_STATUS", OracleDbType.Int32).Value = COM_UP_STATUS;
                cmd.Parameters.Add("C_DOWN_STATUS", OracleDbType.Int32).Value = COM_DOWN_STATUS;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public string UpdateComplianceFlow(string ID, string ENTITY_TYPE_ID, string GROUP_ID, string PREV_GROUP_ID, string NEXT_GROUP_ID)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_UPDATE_COMPLIANCE_FLOW";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("F_ID", OracleDbType.Int32).Value = ID;
                cmd.Parameters.Add("TYPE_ID", OracleDbType.Int32).Value = ENTITY_TYPE_ID;
                cmd.Parameters.Add("GROUP_ID", OracleDbType.Int32).Value = GROUP_ID;
                cmd.Parameters.Add("P_GROUP_ID", OracleDbType.Int32).Value = PREV_GROUP_ID;
                cmd.Parameters.Add("N_GROUP_ID", OracleDbType.Int32).Value = NEXT_GROUP_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public string SubmitNewTeamIdForPostChangesTeamEngReversal(int TEAM_ID, int ENG_ID, int AUDITED_BY_ID, string TEAM_NAME)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_audit_team_postchanges";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("PPNO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("Teamid", OracleDbType.Int32).Value = TEAM_ID;
                cmd.Parameters.Add("AUDID", OracleDbType.Int32).Value = AUDITED_BY_ID;
                cmd.Parameters.Add("TeamName", OracleDbType.Varchar2).Value = TEAM_NAME;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public string AuditEngagementStatusReversal(int ENG_ID, int NEW_STATUS_ID, int PLAN_ID, string COMMENTS)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_audit_engagement_reversal";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("SID", OracleDbType.Int32).Value = NEW_STATUS_ID;
                cmd.Parameters.Add("P_ID", OracleDbType.Int32).Value = PLAN_ID;
                cmd.Parameters.Add("COMMENTS", OracleDbType.Varchar2).Value = COMMENTS;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }

            con.Dispose();
            return resp;

            }

        public string AuditEngagementObsStatusReversal(int ENG_ID, int NEW_STATUS_ID, List<int> OBS_IDS)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            foreach (int obsId in OBS_IDS)
                {
                using (OracleCommand cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_ad.p_audit_observation_reversal";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                    cmd.Parameters.Add("OBS_ID", OracleDbType.Int32).Value = obsId;
                    cmd.Parameters.Add("SID", OracleDbType.Int32).Value = NEW_STATUS_ID;
                    cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                        {
                        resp = rdr["remarks"].ToString();
                        }
                    }
                }

            con.Dispose();
            return resp;

            }

        public List<ObservationNumbersModel> GetObservationNumbersForStatusReversal(int OBS_ID)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<ObservationNumbersModel> resp = new List<ObservationNumbersModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_Get_observvation_no";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("OBS_ID", OracleDbType.Int32).Value = OBS_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ObservationNumbersModel om = new ObservationNumbersModel();
                    om.MEMO_NUMBER = rdr["MEMO_NUMBER"].ToString();
                    om.DRAFT_PARA_NUMBER = rdr["DRAFT_PARA_NO"].ToString();
                    om.FINAL_PARA_NUMBER = rdr["FINAL_PARA_NO"].ToString();
                    resp.Add(om);
                    }
                }


            con.Dispose();
            return resp;

            }

        public string UpdateObservationNumbersForStatusReversal(ObservationNumbersModel onum)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_Update_observation_no";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("M_NO", OracleDbType.Int32).Value = onum.MEMO_NUMBER;
                cmd.Parameters.Add("D_NO", OracleDbType.Int32).Value = onum.DRAFT_PARA_NUMBER;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = onum.FINAL_PARA_NUMBER;
                cmd.Parameters.Add("OBS_ID", OracleDbType.Int32).Value = onum.OBS_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }

            con.Dispose();
            return resp;

            }

        public string UpdateEngagementDatesForStatusReversal(int ENG_ID, DateTime START_DATE, DateTime END_DATE)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_UPDATE_ENG_DATE";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("ST_DATE", OracleDbType.Date).Value = START_DATE;
                cmd.Parameters.Add("ED_DATE", OracleDbType.Date).Value = END_DATE;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }

            con.Dispose();
            return resp;

            }

        public List<HRDesignationWiseRoleModel> GetHRDesignationWiseRoles()
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<HRDesignationWiseRoleModel> resp = new List<HRDesignationWiseRoleModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GET_HR_DESIGNATION_RIGHT";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    HRDesignationWiseRoleModel cm = new HRDesignationWiseRoleModel();

                    cm.ID = Convert.ToInt32(rdr["ID"].ToString());
                    cm.DESIGNATION_CODE = rdr["designationcode"].ToString();
                    cm.DESCRIPTION = rdr["description"].ToString();
                    cm.ROLE_ID = rdr["group_id"].ToString();
                    cm.ROLE = rdr["entity_type"].ToString();
                    cm.ENTITY_TYPE = rdr["sub_entity_type"].ToString();
                    resp.Add(cm);

                    }
                }
            con.Dispose();
            return resp;

            }

        public string AddHRDesignationWiseRoleAssignment(int ASSIGNMENT_ID, int DESIGNATION_ID, int GROUP_ID, string ENTITY_TYPE)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_ADD_HR_DESIGNATION_RIGHT";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("HR_DES_CODE", OracleDbType.Int32).Value = DESIGNATION_ID;
                cmd.Parameters.Add("AIS_GROUP_ID", OracleDbType.Int32).Value = GROUP_ID;
                cmd.Parameters.Add("AIS_SUB_ENTITY_TYPE", OracleDbType.Varchar2).Value = ENTITY_TYPE;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public string UpdateHRDesignationWiseRoleAssignment(int ASSIGNMENT_ID, int DESIGNATION_ID, int GROUP_ID, string ENTITY_TYPE)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_UPDATE_HR_DESIGNATION_RIGHT";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("M_ID", OracleDbType.Int32).Value = ASSIGNMENT_ID;
                cmd.Parameters.Add("HR_DES_CODE", OracleDbType.Int32).Value = DESIGNATION_ID;
                cmd.Parameters.Add("AIS_GROUP_ID", OracleDbType.Int32).Value = GROUP_ID;
                cmd.Parameters.Add("AIS_SUB_ENTITY_TYPE", OracleDbType.Varchar2).Value = ENTITY_TYPE;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }

            con.Dispose();
            return resp;

            }

        public List<ManageObservationModel> GetManageObservationStatus()
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<ManageObservationModel> resp = new List<ManageObservationModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_Get_Obs_Status";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ManageObservationModel m = new ManageObservationModel();
                    m.STATUS_ID = rdr["STATUSID"].ToString();
                    m.STATUS_NAME = rdr["STATUSNAME"].ToString();
                    m.IS_ACTIVE = rdr["ISACTIVE"].ToString();
                    m.CODE = rdr["CODE"].ToString();
                    m.SATISFIED = rdr["SATISFIED"].ToString();
                    resp.Add(m);
                    }
                }
            con.Dispose();
            return resp;

            }

        public string AddManageObservationStatus(ManageObservationModel OBS_STATUS_MODEL)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_add_Obs_status";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("s_name", OracleDbType.Varchar2).Value = OBS_STATUS_MODEL.STATUS_NAME;
                cmd.Parameters.Add("active", OracleDbType.Varchar2).Value = OBS_STATUS_MODEL.IS_ACTIVE;
                cmd.Parameters.Add("s_code", OracleDbType.Varchar2).Value = OBS_STATUS_MODEL.CODE;
                cmd.Parameters.Add("satisfy", OracleDbType.Varchar2).Value = OBS_STATUS_MODEL.SATISFIED;

                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public string UpdateManageObservationStatus(ManageObservationModel OBS_STATUS_MODEL)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_update_Obs_status";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("s_id", OracleDbType.Varchar2).Value = OBS_STATUS_MODEL.STATUS_ID;
                cmd.Parameters.Add("s_name", OracleDbType.Varchar2).Value = OBS_STATUS_MODEL.STATUS_NAME;
                cmd.Parameters.Add("active", OracleDbType.Varchar2).Value = OBS_STATUS_MODEL.IS_ACTIVE;
                cmd.Parameters.Add("s_code", OracleDbType.Varchar2).Value = OBS_STATUS_MODEL.CODE;
                cmd.Parameters.Add("satisfy", OracleDbType.Varchar2).Value = OBS_STATUS_MODEL.SATISFIED;

                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public List<ManageEntAuditDeptModel> GetManageEntityAuditDept()
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<ManageEntAuditDeptModel> resp = new List<ManageEntAuditDeptModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_Get_Entities_Audit_Department";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ManageEntAuditDeptModel m = new ManageEntAuditDeptModel();
                    m.R_ID = rdr["id"].ToString();
                    m.D_ID = rdr["deptid"].ToString();
                    m.D_CODE = rdr["deptcode"].ToString();
                    m.CBAS_CODE = rdr["cbas_code"].ToString();
                    m.ENT_ID = rdr["entity_id"].ToString();
                    m.D_NAME = rdr["deptname"].ToString();
                    m.AUD_ID = rdr["audit_id"].ToString();
                    m.AUDITOR = rdr["auditor"].ToString();
                    m.STATUS = rdr["status"].ToString();
                    resp.Add(m);
                    }
                }
            con.Dispose();
            return resp;

            }

        public string AddManageEntityAuditDepartment(ManageEntAuditDeptModel ENT_AUD_DEPT_MODEL)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_add_Entities_Audit_Department";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("d_id", OracleDbType.Int32).Value = ENT_AUD_DEPT_MODEL.D_ID;
                cmd.Parameters.Add("d_code", OracleDbType.Int32).Value = ENT_AUD_DEPT_MODEL.D_CODE;
                cmd.Parameters.Add("d_name", OracleDbType.Varchar2).Value = ENT_AUD_DEPT_MODEL.D_NAME;
                cmd.Parameters.Add("status", OracleDbType.Varchar2).Value = ENT_AUD_DEPT_MODEL.STATUS;
                cmd.Parameters.Add("cbas_code", OracleDbType.Int32).Value = ENT_AUD_DEPT_MODEL.CBAS_CODE;
                cmd.Parameters.Add("ent_id", OracleDbType.Int32).Value = ENT_AUD_DEPT_MODEL.ENT_ID;
                cmd.Parameters.Add("aud_id", OracleDbType.Int32).Value = ENT_AUD_DEPT_MODEL.AUD_ID;
                cmd.Parameters.Add("auditor", OracleDbType.Varchar2).Value = ENT_AUD_DEPT_MODEL.AUDITOR;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public string UpdateEntityAuditDepartment(ManageEntAuditDeptModel ENT_AUD_DEPT_MODEL)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_update_entities_audit_department";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("r_id", OracleDbType.Int32).Value = ENT_AUD_DEPT_MODEL.R_ID;
                cmd.Parameters.Add("d_id", OracleDbType.Int32).Value = ENT_AUD_DEPT_MODEL.D_ID;
                cmd.Parameters.Add("d_code", OracleDbType.Int32).Value = ENT_AUD_DEPT_MODEL.D_CODE;
                cmd.Parameters.Add("d_name", OracleDbType.Varchar2).Value = ENT_AUD_DEPT_MODEL.D_NAME;
                cmd.Parameters.Add("status", OracleDbType.Varchar2).Value = ENT_AUD_DEPT_MODEL.STATUS;
                cmd.Parameters.Add("cbas_code", OracleDbType.Int32).Value = ENT_AUD_DEPT_MODEL.CBAS_CODE;
                cmd.Parameters.Add("ent_id", OracleDbType.Int32).Value = ENT_AUD_DEPT_MODEL.ENT_ID;
                cmd.Parameters.Add("aud_id", OracleDbType.Int32).Value = ENT_AUD_DEPT_MODEL.AUD_ID;
                cmd.Parameters.Add("auditor", OracleDbType.Varchar2).Value = ENT_AUD_DEPT_MODEL.AUDITOR;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public List<MenuModel> GetAllMenusForAdminPanel()
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<MenuModel> resp = new List<MenuModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GET_ALL_MENU";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    MenuModel m = new MenuModel();
                    m.Menu_Id = Convert.ToInt32(rdr["menu_id"].ToString());
                    m.Menu_Name = rdr["menu_name"].ToString();
                    m.Menu_Order = rdr["menu_order"].ToString();
                    m.Menu_Description = rdr["menu_description"].ToString();
                    resp.Add(m);
                    }
                }
            con.Dispose();
            return resp;

            }

        public List<SubMenuModel> GetSubMenusForAdminPanel(int M_ID)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<SubMenuModel> resp = new List<SubMenuModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GET_SUB_MENUS";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("M_ID", OracleDbType.Int32).Value = M_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    SubMenuModel m = new SubMenuModel();
                    m.SUB_MENU_ID = rdr["SUB_MENU_ID"].ToString();
                    m.MENU_ID = rdr["MENU_ID"].ToString();
                    m.SUB_MENU_NAME = rdr["SUB_MENU_NAME"].ToString();
                    m.SUB_MENU_ORDER = rdr["SUB_MENU_ORDER"].ToString();
                    m.DESCRIPTION = rdr["DESCRIPTION"].ToString();
                    m.STATUS = rdr["STATUS"].ToString();
                    resp.Add(m);
                    }
                }
            con.Dispose();
            return resp;

            }

        public string AddSubMenuForAdminPanel(SubMenuModel sm)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_ADD_NEW_SUB_MENU";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("M_ID", OracleDbType.Int32).Value = sm.MENU_ID;
                cmd.Parameters.Add("SM_NAME", OracleDbType.Varchar2).Value = sm.SUB_MENU_NAME;
                cmd.Parameters.Add("SM_ORDER", OracleDbType.Int32).Value = sm.SUB_MENU_ORDER;
                cmd.Parameters.Add("SM_STATUS", OracleDbType.Varchar2).Value = sm.STATUS;
                cmd.Parameters.Add("SM_DESC", OracleDbType.Varchar2).Value = sm.DESCRIPTION;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public string UpdateSubMenuForAdminPanel(SubMenuModel sm)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_UPDATE_SUB_MENU";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("SM_ID", OracleDbType.Int32).Value = sm.SUB_MENU_ID;
                cmd.Parameters.Add("M_ID", OracleDbType.Int32).Value = sm.MENU_ID;
                cmd.Parameters.Add("SM_NAME", OracleDbType.Varchar2).Value = sm.SUB_MENU_NAME;
                cmd.Parameters.Add("SM_ORDER", OracleDbType.Int32).Value = sm.SUB_MENU_ORDER;
                cmd.Parameters.Add("SM_STATUS", OracleDbType.Varchar2).Value = sm.STATUS;
                cmd.Parameters.Add("SM_DESC", OracleDbType.Varchar2).Value = sm.DESCRIPTION;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public List<MenuPagesAssignmentModel> GetMenuPagesForAdminPanel(int M_ID, int SM_ID)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<MenuPagesAssignmentModel> resp = new List<MenuPagesAssignmentModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GET_ALL_PAGES";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("M_ID", OracleDbType.Int32).Value = M_ID;
                cmd.Parameters.Add("SM_ID", OracleDbType.Int32).Value = SM_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    MenuPagesAssignmentModel m = new MenuPagesAssignmentModel();
                    m.P_ID = rdr["id"].ToString();
                    m.M_ID = rdr["menu_id"].ToString();
                    m.SM_ID = rdr["sub_menu_id"].ToString();
                    m.SM_NAME = rdr["sub_menu_name"].ToString();
                    m.P_NAME = rdr["page_name"].ToString();
                    m.P_PATH = rdr["page_path"].ToString();
                    m.P_ORDER = rdr["page_order"].ToString();
                    m.P_STATUS = rdr["status"].ToString();
                    m.P_HIDE_MENU = rdr["hide_menu"].ToString();

                    resp.Add(m);
                    }
                }
            con.Dispose();
            return resp;

            }

        public string AddMenuPageForAdminPanel(MenuPagesAssignmentModel pageModel)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_ADD_NEW_PAGE";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("M_ID", OracleDbType.Int32).Value = pageModel.M_ID;
                cmd.Parameters.Add("SM_ID", OracleDbType.Int32).Value = pageModel.SM_ID;
                cmd.Parameters.Add("P_NAME", OracleDbType.Varchar2).Value = pageModel.P_NAME;
                cmd.Parameters.Add("P_PATH", OracleDbType.Varchar2).Value = pageModel.P_PATH;
                cmd.Parameters.Add("P_ORDER", OracleDbType.Int32).Value = pageModel.P_ORDER;
                cmd.Parameters.Add("P_STATUS", OracleDbType.Varchar2).Value = pageModel.P_STATUS;
                cmd.Parameters.Add("P_HIDE_MENU", OracleDbType.Int32).Value = pageModel.P_HIDE_MENU;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public string UpdateMenuPageForAdminPanel(MenuPagesAssignmentModel pageModel)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_UPDATE_PAGE";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_ID", OracleDbType.Int32).Value = pageModel.P_ID;
                cmd.Parameters.Add("M_ID", OracleDbType.Int32).Value = pageModel.M_ID;
                cmd.Parameters.Add("SM_ID", OracleDbType.Int32).Value = pageModel.SM_ID;
                cmd.Parameters.Add("P_NAME", OracleDbType.Varchar2).Value = pageModel.P_NAME;
                cmd.Parameters.Add("P_PATH", OracleDbType.Varchar2).Value = pageModel.P_PATH;
                cmd.Parameters.Add("P_ORDER", OracleDbType.Int32).Value = pageModel.P_ORDER;
                cmd.Parameters.Add("P_STATUS", OracleDbType.Varchar2).Value = pageModel.P_STATUS;
                cmd.Parameters.Add("P_HIDE_MENU", OracleDbType.Int32).Value = pageModel.P_HIDE_MENU;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public string UpdateComplianceUnit(int ENT_ID, int AUD_ID, string COMP_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_UPDATE_ENTITY_COMP";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = ENT_ID;
                cmd.Parameters.Add("AUDITOR", OracleDbType.Int32).Value = AUD_ID;
                cmd.Parameters.Add("COMPLIANCE", OracleDbType.Varchar2).Value = COMP_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public string AddAnnexure(string ANNEX_CODE = "", string HEADING = "", int PROCESS_ID = 0, int FUNCTION_OWNER_ID = 0, int FUNCTION_ID_1 = 0, int FUNCTION_ID_2 = 0, int RISK_ID = 0, string MAX_NUMBER = "", string GRAVITY = "", string WEIGHTAGE = "")
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_ADD_ANNEXURE";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("CODE", OracleDbType.Varchar2).Value = ANNEX_CODE;
                cmd.Parameters.Add("TITLE", OracleDbType.Varchar2).Value = HEADING;
                cmd.Parameters.Add("RISK_ID", OracleDbType.Int32).Value = RISK_ID;
                cmd.Parameters.Add("OWNER", OracleDbType.Int32).Value = FUNCTION_OWNER_ID;
                cmd.Parameters.Add("FUNCTION_ID_1", OracleDbType.Int32).Value = FUNCTION_ID_1;
                cmd.Parameters.Add("FUNCTION_ID_2", OracleDbType.Int32).Value = FUNCTION_ID_2;
                cmd.Parameters.Add("PROCESS_ID", OracleDbType.Int32).Value = PROCESS_ID;
                cmd.Parameters.Add("max_num", OracleDbType.Varchar2).Value = MAX_NUMBER;
                cmd.Parameters.Add("weightage_num", OracleDbType.Varchar2).Value = WEIGHTAGE;
                cmd.Parameters.Add("gravity_num", OracleDbType.Varchar2).Value = GRAVITY;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public string UpdateAnnexure(int ANNEX_ID = 0, string HEADING = "", int PROCESS_ID = 0, int FUNCTION_OWNER_ID = 0, int FUNCTION_ID_1 = 0, int FUNCTION_ID_2 = 0, int RISK_ID = 0, string MAX_NUMBER = "", string GRAVITY = "", string WEIGHTAGE = "")
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_update_annexure";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ANEXX", OracleDbType.Int32).Value = ANNEX_ID;
                cmd.Parameters.Add("TITLE", OracleDbType.Varchar2).Value = HEADING;
                cmd.Parameters.Add("RISK_ID", OracleDbType.Int32).Value = RISK_ID;
                cmd.Parameters.Add("OWNER", OracleDbType.Int32).Value = FUNCTION_OWNER_ID;
                cmd.Parameters.Add("FUNCTION_ID_1", OracleDbType.Int32).Value = FUNCTION_ID_1;
                cmd.Parameters.Add("FUNCTION_ID_2", OracleDbType.Int32).Value = FUNCTION_ID_2;
                cmd.Parameters.Add("PROCESS_ID", OracleDbType.Int32).Value = PROCESS_ID;
                cmd.Parameters.Add("max_num", OracleDbType.Varchar2).Value = MAX_NUMBER;
                cmd.Parameters.Add("weightage_num", OracleDbType.Varchar2).Value = WEIGHTAGE;
                cmd.Parameters.Add("gravity_num", OracleDbType.Varchar2).Value = GRAVITY;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public List<AuditeeEntitiesModel> GetEntityForAuditReconsilition()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<AuditeeEntitiesModel> entitiesList = new List<AuditeeEntitiesModel>();
            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GET_ENTITY_FOR_PARA_Reconsilation";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditeeEntitiesModel entity = new AuditeeEntitiesModel();
                    entity.NAME = rdr["name"].ToString();
                    entity.CODE = Convert.ToInt32(rdr["entity_id"].ToString());
                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }

        public string GenerateTraditionalRiskRatingofEngagement(int ENG_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_add_branch_risk_rating";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public List<TraditionalRiskRatingModel> ViewTraditionalRiskRatingofEngagement(int ENG_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<TraditionalRiskRatingModel> resp = new List<TraditionalRiskRatingModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_get_traditional_risk_rating";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    TraditionalRiskRatingModel r = new TraditionalRiskRatingModel();
                    r.MAIN_PROCESS = rdr["MAIN_PROCESS"].ToString();
                    r.MAX_NUMBER = rdr["MAX_NUMBER"].ToString();
                    r.WEIGHTAGE_AVERAGE = rdr["WEIGHTAGE_AVERAGE"].ToString();
                    r.WEIGHTED_AVERAGE_MARKS = rdr["WEIGHTED_AVERAGE_MARKS"].ToString();
                    r.RISK_MODEL = rdr["RISK_MODEL"].ToString();
                    r.NO_OF_OBSERVATIONS = rdr["number_of_observations"].ToString();
                    r.GRAVITY_RISK = rdr["GRAVITY_RISK"].ToString();
                    r.RISK_BASED_MARKS = rdr["RISK_BASED_MARKS"].ToString();
                    r.CIA_MARKS = rdr["CIA_MARKS"].ToString();
                    resp.Add(r);
                    }
                }
            con.Dispose();
            return resp;

            }

        public string GenerateAnnexureRiskRatingofEngagement(int ENG_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_add_branch_annex_risk_rating";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public List<TraditionalRiskRatingModel> ViewAnnexureRiskRatingofEngagement(int ENG_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<TraditionalRiskRatingModel> resp = new List<TraditionalRiskRatingModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_get_annexure_risk_rating";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    TraditionalRiskRatingModel r = new TraditionalRiskRatingModel();
                    r.MAIN_PROCESS = rdr["MAIN_PROCESS"].ToString();
                    r.MAX_NUMBER = rdr["MAX_NUMBER"].ToString();
                    r.WEIGHTAGE_AVERAGE = rdr["WEIGHTAGE_AVERAGE"].ToString();
                    r.WEIGHTED_AVERAGE_MARKS = rdr["WEIGHTED_AVERAGE_MARKS"].ToString();
                    r.RISK_MODEL = rdr["RISK_MODEL"].ToString();
                    r.GRAVITY_RISK = rdr["GRAVITY_RISK"].ToString();
                    r.RISK_BASED_MARKS = rdr["RISK_BASED_MARKS"].ToString();
                    resp.Add(r);
                    }
                }
            con.Dispose();
            return resp;

            }

        public List<RiskRatingModelForBranchesWorking> GetRiskRatingModelForBranchesWorking(int ENG_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<RiskRatingModelForBranchesWorking> resp = new List<RiskRatingModelForBranchesWorking>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_get_new_risk_model ";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    RiskRatingModelForBranchesWorking auditChecklist = new RiskRatingModelForBranchesWorking();
                    auditChecklist.MainProcessRiskSequence = rdr["risk_sequence"].ToString();
                    auditChecklist.MainProcess = rdr["Main_process"].ToString();
                    auditChecklist.MainProcessWeightAssigned = rdr["weight_assigned"].ToString();
                    auditChecklist.SubProcessRiskSequence = rdr["risk_sequence"].ToString();
                    auditChecklist.SubProcess = rdr["heading"].ToString();
                    auditChecklist.SubProcessWeightAssigned = rdr["weight_assigned"].ToString();
                    auditChecklist.High = rdr["High"].ToString();
                    auditChecklist.Medium = rdr["medium"].ToString();
                    auditChecklist.Low = rdr["Low"].ToString();
                    auditChecklist.TotalNoOfTest = rdr["total_no_of_test"].ToString();
                    auditChecklist.AvailableWeightedScore = rdr["AVAILABLE_WEIGHTED_SCORE"].ToString();
                    auditChecklist.AvailableProcessScore = rdr["AVAILABLE_PROCESS_SCORE"].ToString();
                    auditChecklist.TotalHigh = rdr["HIGH"].ToString();
                    auditChecklist.TotalMedium = rdr["MEDIUM"].ToString();
                    auditChecklist.TotalLow = rdr["LOW"].ToString();
                    auditChecklist.TotalObservations = rdr["TOTAL_OBS"].ToString();
                    auditChecklist.TotalScoreSubProcess = rdr["TOTAL_SCORE_SUB_PROCESS"].ToString();
                    auditChecklist.WeightedAverageScore = rdr["WEIGHTED_AVERAGE_SCORE"].ToString();
                    auditChecklist.TotalScoreProcess = rdr["TOTAL_SCORE_PROCESS"].ToString();
                    auditChecklist.WeightedAverageScoreOverall = rdr["WEIGHTED_AVERAGE_SCORE_OVERALL"].ToString();

                    resp.Add(auditChecklist);
                    }
                }
            con.Dispose();
            return resp;

            }

        public List<ComplianceHierarchyModel> GetComplianceHierarchies()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<ComplianceHierarchyModel> resp = new List<ComplianceHierarchyModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.p_get_compliance_hierarchy ";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ComplianceHierarchyModel complianceHierarchyModel = new ComplianceHierarchyModel();
                    complianceHierarchyModel.ENTITY_ID = rdr["ENTITY_ID"].ToString();
                    complianceHierarchyModel.COMPLIANCE_UNIT = rdr["COMPLIANCE_UNIT"].ToString();
                    complianceHierarchyModel.APPROVER_PPNO = rdr["APPROVER_PPNO"].ToString();
                    complianceHierarchyModel.APPROVER_NAME = rdr["APPROVER_NAME"].ToString();
                    complianceHierarchyModel.REVIEWER_PPNO = rdr["REVIEWER_PPNO"].ToString();
                    complianceHierarchyModel.REVIEWER_NAME = rdr["REVIEWER_NAME"].ToString();
                    complianceHierarchyModel.COM_KEY = rdr["COM_KEY"].ToString();
                    resp.Add(complianceHierarchyModel);
                    }
                }
            con.Dispose();
            return resp;

            }

        public string AddComplianceHierarchy(int ENTITY_ID, string REVIEWER_PP, string AUTHORIZER_PP)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_ADD_COM_OFFICER";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("AP_P_NO", OracleDbType.Int32).Value = AUTHORIZER_PP;
                cmd.Parameters.Add("RE_P_NO", OracleDbType.Int32).Value = REVIEWER_PP;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public string UpdateComplianceHierarchy(int ENTITY_ID, string REVIEWER_PP, string AUTHORIZER_PP, string COMPLIANCE_KEY)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_UPDATE_COM_OFFICER";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("AP_P_NO", OracleDbType.Int32).Value = AUTHORIZER_PP;
                cmd.Parameters.Add("RE_P_NO", OracleDbType.Int32).Value = REVIEWER_PP;
                cmd.Parameters.Add("E_COM_KEY", OracleDbType.Varchar2).Value = COMPLIANCE_KEY;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public List<AuditeeEntitiesModel> GetGMOffices()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            List<AuditeeEntitiesModel> AZList = new List<AuditeeEntitiesModel>();
            var loggedInUser = sessionHandler.GetUserOrThrow();


            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GET_GM_OFFICE";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditeeEntitiesModel z = new AuditeeEntitiesModel();
                    z.ENTITY_ID = Convert.ToInt32(rdr["entity_id"]);
                    z.NAME = rdr["name"].ToString();
                    AZList.Add(z);
                    }
                }
            con.Dispose();
            return AZList;
            }

        public List<AuditeeEntitiesModel> GetReportingOffices()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            List<AuditeeEntitiesModel> AZList = new List<AuditeeEntitiesModel>();
            var loggedInUser = sessionHandler.GetUserOrThrow();


            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GET_RPT_OFFICE";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditeeEntitiesModel z = new AuditeeEntitiesModel();
                    z.ENTITY_ID = Convert.ToInt32(rdr["entity_id"]);
                    z.NAME = rdr["name"].ToString();
                    AZList.Add(z);
                    }
                }
            con.Dispose();
            return AZList;
            }

        public string UpdateGMOffice(int GM_OFFICE_ID, int ENTITY_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_UPDATE_GM_OFFICE_RELATIONSHIP";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("GM", OracleDbType.Int32).Value = GM_OFFICE_ID;
                cmd.Parameters.Add("ENT", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public string UpdateReportingLine(int REP_OFFICE_ID, int ENTITY_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_UPDATE_RPT_OFFICE_RELATIONSHIP";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("RPT", OracleDbType.Int32).Value = REP_OFFICE_ID;
                cmd.Parameters.Add("ENT", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public List<AISPostComplianceModel> GetAisPostComplianceDetails(int ENT)
            {
            List<AISPostComplianceModel> list = new List<AISPostComplianceModel>();
            var con = this.DatabaseConnection();
           
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_get_latest_para_details";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT", OracleDbType.Int32).Value = ENT;
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AISPostComplianceModel m = new AISPostComplianceModel();
                    m.COMID = rdr["COMID"].ToString();
                    m.AUDITPERIOD = rdr["AUDITPERIOD"].ToString();
                    m.AUDITEDBY = rdr["AUDITEDBY"].ToString();
                    m.ENTITYTYPEID = rdr["ENTITYTYPEID"].ToString();
                    m.COMCYCLE = rdr["COMCYCLE"].ToString();
                    m.COMSTATUS = rdr["COMSTATUS"].ToString();
                    m.COMSTAGE = rdr["COMSTAGE"].ToString();
                    m.PARASTATUS = rdr["PARASTATUS"].ToString();
                    m.PARANO = rdr["PARANO"].ToString();
                    m.GISTOFPARAS = rdr["GISTOFPARAS"].ToString();
                    m.IND = rdr["IND"].ToString();
                    m.RISK = rdr["RISK"].ToString();
                    list.Add(m);
                    }
                }
            con.Dispose();
            return list;
            }

        public string UpdateAisPostCompliance(AISPostComplianceModel m)
            {
            string resp = "";
            var con = this.DatabaseConnection();
           
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_Update_para_AIS_post_compliance";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ca_com_id", OracleDbType.Int32).Value = Convert.ToInt32(m.COMID);
                cmd.Parameters.Add("ca_audit_period", OracleDbType.Varchar2).Value = m.AUDITPERIOD;
                cmd.Parameters.Add("ca_gist_of_paras", OracleDbType.Varchar2).Value = m.GISTOFPARAS;
                cmd.Parameters.Add("ca_audited_by", OracleDbType.Int32).Value = Convert.ToInt32(m.AUDITEDBY);
                cmd.Parameters.Add("ca_entity_type_id", OracleDbType.Int32).Value = Convert.ToInt32(m.ENTITYTYPEID);
                cmd.Parameters.Add("ca_com_cycle", OracleDbType.Int32).Value = Convert.ToInt32(m.COMCYCLE);
                cmd.Parameters.Add("ca_com_status", OracleDbType.Int32).Value = Convert.ToInt32(m.COMSTATUS);
                cmd.Parameters.Add("ca_com_stage", OracleDbType.Int32).Value = Convert.ToInt32(m.COMSTAGE);
                cmd.Parameters.Add("ca_para_status", OracleDbType.Int32).Value = Convert.ToInt32(m.PARASTATUS);
                cmd.Parameters.Add("ca_para_no", OracleDbType.Varchar2).Value = m.PARANO;
                cmd.Parameters.Add("ca_ind", OracleDbType.Varchar2).Value = m.IND;
                cmd.Parameters.Add("ca_risk", OracleDbType.Int32).Value = Convert.ToInt32(m.RISK);
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr[0].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public List<FADAuditEmpModel> GetAuditEmployees()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<FADAuditEmpModel> list = new List<FADAuditEmpModel>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_Get_Audit_EMP";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("io_Cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    FADAuditEmpModel mEmp = new FADAuditEmpModel();
                    mEmp.ID = rdr["ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ID"]);
                    mEmp.PPNO = rdr["PPNO"].ToString();
                    mEmp.NAME = rdr["NAME"].ToString();
                    mEmp.RANK = rdr["RANK"].ToString();
                    mEmp.DESIGNATION = rdr["DESIGNATION"].ToString();
                    mEmp.PLACEMENT = rdr["PLACEMENT"].ToString();
                    mEmp.QUALIFICATION = rdr["QUALIFICATION"].ToString();
                    mEmp.SPECIALIZATION = rdr["SPECIALIZATION"].ToString();
                    mEmp.CERTIFICATION = rdr["CERTIFICATION"].ToString();
                    mEmp.TOTAL_EXPERIENCE = rdr["TOTAL_EXPERIENCE"].ToString();
                    mEmp.AUDIT_EXPERIENCE = rdr["AUDIT_EXPERIENCE"].ToString();
                    list.Add(mEmp);
                    }
                }
            con.Dispose();
            return list;
            }

        public string UpdateAuditEmployee(FADAuditEmpModel m)
            {
            string resp = "";
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_update_audit_emp";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("id", OracleDbType.Int32).Value = m.ID;
                cmd.Parameters.Add("ppno", OracleDbType.Varchar2).Value = m.PPNO;
                cmd.Parameters.Add("rank", OracleDbType.Varchar2).Value = m.RANK;
                cmd.Parameters.Add("designation", OracleDbType.Varchar2).Value = m.DESIGNATION;
                cmd.Parameters.Add("placement", OracleDbType.Varchar2).Value = m.PLACEMENT;
                cmd.Parameters.Add("qualification", OracleDbType.Varchar2).Value = m.QUALIFICATION;
                cmd.Parameters.Add("specialization", OracleDbType.Varchar2).Value = m.SPECIALIZATION;
                cmd.Parameters.Add("certification", OracleDbType.Varchar2).Value = m.CERTIFICATION;
                cmd.Parameters.Add("tot_exp", OracleDbType.Varchar2).Value = m.TOTAL_EXPERIENCE;
                cmd.Parameters.Add("audit_exp", OracleDbType.Varchar2).Value = m.AUDIT_EXPERIENCE;
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    resp = rdr[0].ToString();
                }
            con.Dispose();
            return resp;
            }

        public List<FADAuditManpowerModel> GetAuditManpower()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<FADAuditManpowerModel> list = new List<FADAuditManpowerModel>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_Get_Audit_Manpower";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("io_Cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    FADAuditManpowerModel mm = new FADAuditManpowerModel();
                    mm.ID = rdr["ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ID"]);
                    mm.COMPANY = rdr["COMPANY"].ToString();
                    mm.RANK = rdr["RANK"].ToString();
                    mm.PLACEMENT = rdr["PLACEMENT"].ToString();
                    mm.EXISTING = rdr["EXISTING"].ToString();
                    mm.ADDITIONAL_REQUIRED = rdr["ADDITIONAL_REQUIRED"].ToString();
                    list.Add(mm);
                    }
                }
            con.Dispose();
            return list;
            }

        public string UpdateAuditManpower(FADAuditManpowerModel m)
            {
            string resp = "";
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_update_audit_manpower";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("id", OracleDbType.Int32).Value = m.ID;
                cmd.Parameters.Add("company", OracleDbType.Varchar2).Value = m.COMPANY;
                cmd.Parameters.Add("rank", OracleDbType.Varchar2).Value = m.RANK;
                cmd.Parameters.Add("placement", OracleDbType.Varchar2).Value = m.PLACEMENT;
                cmd.Parameters.Add("existing", OracleDbType.Varchar2).Value = m.EXISTING;
                cmd.Parameters.Add("additional", OracleDbType.Varchar2).Value = m.ADDITIONAL_REQUIRED;
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    resp = rdr[0].ToString();
                }
            con.Dispose();
            return resp;
            }

        public List<FADAuditBudgetModel> GetAuditBudget()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<FADAuditBudgetModel> list = new List<FADAuditBudgetModel>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_Get_Audit_budget";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("io_Cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    FADAuditBudgetModel mb = new FADAuditBudgetModel();
                    mb.ID = rdr["ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ID"]);
                    mb.GL_CODE = rdr["GL_CODE"].ToString();
                    mb.GL_HEADING = rdr["GL_HEADING"].ToString();
                    mb.EXISTING = rdr["EXISTING"].ToString();
                    mb.UTILIZATION = rdr["UTILIZATION"].ToString();
                    mb.REMAND = rdr["REMAND"].ToString();
                    mb.RATIONALIZATION = rdr["RATIONALIZATION"].ToString();
                    mb.STATUS = rdr["STATUS"].ToString();
                    list.Add(mb);
                    }
                }
            con.Dispose();
            return list;
            }

        public string UpdateAuditBudget(FADAuditBudgetModel m)
            {
            string resp = "";
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_update_audit_budget";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("id", OracleDbType.Int32).Value = m.ID;
                cmd.Parameters.Add("gl_code", OracleDbType.Varchar2).Value = m.GL_CODE;
                cmd.Parameters.Add("gl_heading", OracleDbType.Varchar2).Value = m.GL_HEADING;
                cmd.Parameters.Add("existing", OracleDbType.Varchar2).Value = m.EXISTING;
                cmd.Parameters.Add("utilization", OracleDbType.Varchar2).Value = m.UTILIZATION;
                cmd.Parameters.Add("remand", OracleDbType.Varchar2).Value = m.REMAND;
                cmd.Parameters.Add("rationalization", OracleDbType.Varchar2).Value = m.RATIONALIZATION;
                cmd.Parameters.Add("status", OracleDbType.Varchar2).Value = m.STATUS;
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    resp = rdr[0].ToString();
                }
            con.Dispose();
            return resp;
            }

        public List<DropDownModel> GetHrRanks()
            {
            List<DropDownModel> list = new List<DropDownModel>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_get_hr_rank";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    DropDownModel d = new DropDownModel();
                    d.ID = rdr["ID"].ToString();
                    d.DESCRIPTION = rdr["DESCRIPTION"].ToString();
                    list.Add(d);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<DropDownModel> GetHrDesignations()
            {
            List<DropDownModel> list = new List<DropDownModel>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_get_hr_designation";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    DropDownModel d = new DropDownModel();
                    d.ID = rdr["ID"].ToString();
                    d.DESCRIPTION = rdr["DESCRIPTION"].ToString();
                    list.Add(d);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<DropDownModel> GetHrPosting()
            {
            List<DropDownModel> list = new List<DropDownModel>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_get_hr_posting";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    DropDownModel d = new DropDownModel();
                    d.ID = rdr["ID"].ToString();
                    d.DESCRIPTION = rdr["DESCRIPTION"].ToString();
                    list.Add(d);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<DropDownModel> GetQualifications()
            {
            List<DropDownModel> list = new List<DropDownModel>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_get_qualification";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    DropDownModel d = new DropDownModel();
                    d.ID = rdr["ID"].ToString();
                    d.DESCRIPTION = rdr["DESCRIPTION"].ToString();
                    list.Add(d);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<DropDownModel> GetQualificationSpecialization()
            {
            List<DropDownModel> list = new List<DropDownModel>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_get_qualification_specialization";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    DropDownModel d = new DropDownModel();
                    d.ID = rdr["ID"].ToString();
                    d.DESCRIPTION = rdr["DESCRIPTION"].ToString();
                    list.Add(d);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<DropDownModel> GetCertifications()
            {
            List<DropDownModel> list = new List<DropDownModel>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_get_certification";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    DropDownModel d = new DropDownModel();
                    d.ID = rdr["ID"].ToString();
                    d.DESCRIPTION = rdr["DESCRIPTION"].ToString();
                    list.Add(d);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<DropDownModel> GetGLHeads()
            {
            List<DropDownModel> list = new List<DropDownModel>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_get_GL_Heads";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    DropDownModel d = new DropDownModel();
                    d.ID = rdr["GL_CODE"].ToString();
                    d.DESCRIPTION = rdr["GL_HEADING"].ToString();
                    list.Add(d);
                    }
                }
            con.Dispose();
            return list;
            }

        public PublicHolidayModel AddPublicHoliday(PublicHolidayModel model)
            {
            // Session pattern (if needed for auditing/logged-in info)
            var sessionHandler = CreateSessionHandler();

            var con = this.DatabaseConnection();
           

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_AD.P_INSERT_PUBLIC_HOLIDAY";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();

                cmd.Parameters.Add("p_holiday_date", OracleDbType.Date).Value = model.HOLIDAY_DATE ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_is_weekend", OracleDbType.Char).Value = model.IS_WEEKEND;
                cmd.Parameters.Add("p_is_holiday", OracleDbType.Char).Value = model.IS_HOLIDAY;
                cmd.Parameters.Add("p_holiday_name", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(model.HOLIDAY_NAME) ? (object)DBNull.Value : model.HOLIDAY_NAME;

                cmd.ExecuteNonQuery();
                }

            con.Dispose();
            return model;
            }

        public List<PublicHolidayModel> GetAllPublicHolidays(int year = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<PublicHolidayModel> list = new List<PublicHolidayModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GET_PUBLIC_HOLIDAYS";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("p_year", OracleDbType.Int32).Value = year == 0 ? (object)DBNull.Value : year;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    PublicHolidayModel ad = new PublicHolidayModel();
                    ad.ID = Convert.ToInt32(rdr["ID"]);
                    ad.HOLIDAY_DATE = Convert.ToDateTime(rdr["HOLIDAY_DATE"]);
                    ad.HOLIDAY_YEAR = Convert.ToInt32(rdr["HOLIDAY_YEAR"]);
                    ad.IS_WEEKEND = rdr["IS_WEEKEND"].ToString();
                    ad.IS_HOLIDAY = rdr["IS_HOLIDAY"].ToString();
                    ad.HOLIDAY_NAME = rdr["HOLIDAY_NAME"]?.ToString();
                    list.Add(ad);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<PublicHolidayModel> CheckIfHolidayOrWeekend(String dat)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<PublicHolidayModel> list = new List<PublicHolidayModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_GET_PUBLIC_HOLIDAY_DAY";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("p_day", OracleDbType.Single).Value = dat;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    PublicHolidayModel ad = new PublicHolidayModel();
                    ad.IS_HOLIDAY = rdr["holiday"].ToString();
                    ad.IS_WEEKEND = rdr["weekend"].ToString();
                    list.Add(ad);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<VersionHistoryModel> GetAllVersionHistory()
            {
            var sessionHandler = CreateSessionHandler();

            var con = this.DatabaseConnection();
           
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<VersionHistoryModel> list = new List<VersionHistoryModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_AD.P_GET_VERSION_HISTORY";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    VersionHistoryModel ad = new VersionHistoryModel();
                    ad.VersionId = Convert.ToInt32(rdr["VERSION_ID"]);
                    ad.VersionNo = rdr["VERSION_NO"]?.ToString();
                    ad.ReleaseDate = Convert.ToDateTime(rdr["RELEASE_DATE"]);
                    ad.Description = rdr["DESCRIPTION"]?.ToString();
                    ad.ReleasedBy = rdr["RELEASED_BY"]?.ToString();
                    ad.CreatedOn = Convert.ToDateTime(rdr["CREATED_ON"]);
                    ad.UpdatedOn = rdr["UPDATED_ON"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["UPDATED_ON"]);
                    ad.IsActive = rdr["IS_ACTIVE"]?.ToString();
                    list.Add(ad);
                    }
                }
            con.Dispose();
            return list;
            }

        public string AddVersionHistory(VersionHistoryModel model)
            {
            var sessionHandler = CreateSessionHandler();

            var con = this.DatabaseConnection();
           
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string result = "";

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_AD.P_ADD_VERSION_HISTORY";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("i_version_no", OracleDbType.Varchar2).Value = model.VersionNo;
                cmd.Parameters.Add("i_release_date", OracleDbType.Date).Value = model.ReleaseDate ?? (object)DBNull.Value;
                cmd.Parameters.Add("i_description", OracleDbType.Varchar2).Value = model.Description ?? "";
                cmd.Parameters.Add("i_released_by", OracleDbType.Varchar2).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("o_result", OracleDbType.Varchar2, 4000).Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();
                result = cmd.Parameters["o_result"].Value?.ToString();
                }
            con.Dispose();
            return result;
            }

        public string UpdateVersionHistory(VersionHistoryModel model)
            {
            var sessionHandler = CreateSessionHandler();

            var con = this.DatabaseConnection();
           
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string result = "";

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_AD.P_UPDATE_VERSION_HISTORY";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("i_version_id", OracleDbType.Int32).Value = model.VersionId ?? (object)DBNull.Value;
                cmd.Parameters.Add("i_version_no", OracleDbType.Varchar2).Value = model.VersionNo;
                cmd.Parameters.Add("i_release_date", OracleDbType.Date).Value = model.ReleaseDate ?? (object)DBNull.Value;
                cmd.Parameters.Add("i_description", OracleDbType.Varchar2).Value = model.Description ?? "";
                cmd.Parameters.Add("i_released_by", OracleDbType.Varchar2).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("i_is_active", OracleDbType.Char).Value = model.IsActive;
                cmd.Parameters.Add("o_result", OracleDbType.Varchar2, 4000).Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();
                result = cmd.Parameters["o_result"].Value?.ToString();
                }
            con.Dispose();
            return result;
            }
        public string UpdateGMAndReportingLineOffice(int ENTITY_ID, int GM_OFF_ID, int REP_OFF_ID)
            {
            string resp = "";
            if (GM_OFF_ID > 0)
                this.UpdateGMOffice(GM_OFF_ID, ENTITY_ID);

            if (REP_OFF_ID > 0)
                this.UpdateReportingLine(REP_OFF_ID, ENTITY_ID);

            return resp;
            }

        public List<UserEntityModel> GetUserEntities(int userId)
            {
            var list = new List<UserEntityModel>();

            using var con = this.DatabaseConnection();
            using var command = con.CreateCommand();
            command.CommandText = "PKG_AD.P_GET_USER_ENTITIES";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("p_user_id", OracleDbType.Int32).Value = userId;
            command.Parameters.Add("o_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = command.ExecuteReader();

            while (reader.Read())
                {
                list.Add(new UserEntityModel
                    {
                    Id = Convert.ToInt32(reader["ID"]),
                    EntityId = Convert.ToInt32(reader["ENTITY_ID"]),
                    EntityName = reader["NAME"].ToString(),
                    RoleId = Convert.ToInt32(reader["ROLE_ID"]),
                    RoleName = reader["ROLE_NAME"].ToString(),
                    IsPrimary = reader["IS_PRIMARY"].ToString()
                    });
                }

            return list;
            }


        }
    }
