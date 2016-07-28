using RolesDbApp;
using System;
using System.Collections.Generic;
using System.Linq;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Data.Linq;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Security.Data;
using Telerik.Sitefinity.Security.Model;

namespace SitefinityWebApp
{
    public class CustomRoleProvider : RoleDataProvider, IOpenAccessDataProvider
    {
        public override Telerik.Sitefinity.Security.Model.Role CreateRole(Guid Id, string roleName)
        {
            var role = new Telerik.Sitefinity.Security.Model.Role();

            using (var entity = new RolesDbAppContext())
            {
                var roleExist = entity.Roles.Where(r => r.Name == roleName).FirstOrDefault();
                if (roleExist != null)
                {
                    throw new ArgumentException("role name exists", "roleName");
                }

                var roleExistId = entity.Roles.Where(r => r.Id == Id).FirstOrDefault();
                if (roleExistId != null)
                {
                    throw new ArgumentException("Id exists", "Id");
                }

                var newRole = entity.Roles.Create();

                newRole.Id = Id;
                newRole.Name = roleName;
                entity.Roles.Add(newRole);
                entity.SaveChanges();

                role.Id = Id;
                role.Name = roleName;
                role.ApplicationName = this.ApplicationName;

                return role;
            }
        }

        public override Telerik.Sitefinity.Security.Model.Role CreateRole(string roleName)
        {
            return this.CreateRole(this.GetNewGuid(), roleName);
        }

        public override void Delete(Telerik.Sitefinity.Security.Model.Role item)
        {
            using (var entity = new RolesDbAppContext())
            {
                var role = entity.Roles.Where(r => r.Name == item.Name).FirstOrDefault();

                var scope = this.GetContext();
                try
                {
                    var itemFromContext = scope.GetItemById(item.GetType(), item.Id.ToString());
                    if (itemFromContext != null)
                    {
                        scope.Remove(itemFromContext);
                    }
                }
                catch (Exception ex)
                {
                }
                
                entity.Roles.Remove(role);

                entity.SaveChanges();
            }
        }

        public override Telerik.Sitefinity.Security.Model.Role GetRole(Guid Id)
        {
            var newRole = new Telerik.Sitefinity.Security.Model.Role();
            using (var entity = new RolesDbAppContext())
            {
                var role = entity.Roles.Where(r => r.Id == Id).FirstOrDefault();
                newRole.Id = role.Id;
                newRole.Name = role.Name;
                newRole.ApplicationName = this.ApplicationName;
            }

            return newRole;
        }

        public override IQueryable<Telerik.Sitefinity.Security.Model.Role> GetRoles()
        {
            List<Telerik.Sitefinity.Security.Model.Role> listEntities = new List<Telerik.Sitefinity.Security.Model.Role>();

            using (var entity = new RolesDbAppContext())
            {
                var rolesQuery = from r in entity.Roles
                                 select new Telerik.Sitefinity.Security.Model.Role()
                                 {
                                     Id = r.Id,
                                     Name = r.Name,
                                     ApplicationName = this.ApplicationName
                                 };

                listEntities = rolesQuery.ToList();
            }

            return listEntities.AsQueryable();
        }

        public override Telerik.Sitefinity.Security.Model.UserLink CreateUserLink(Guid id)
        {         
            if (id == Guid.Empty)
                throw new ArgumentNullException("id");

            var item = new UserLink()
            {
                ApplicationName = this.ApplicationName,
                Id = id
            };

            ((IDataItem)item).Provider = this;

            return item;
        }

        public override ManagerInfo GetManagerInfo(string managerType, string providerName)
        {
            var manInfor = this.UserManagerInfo;
            manInfor.ManagerType = managerType;
            manInfor.ProviderName = providerName;
            return manInfor;
        }

        public override ManagerInfo GetManagerInfo(Guid Id)
        {
            var manInfor = this.UserManagerInfo;
            manInfor.Id = Id;
            return manInfor;
        }

        public override System.Collections.IList GetDirtyItems()
        {
            return new System.Collections.Generic.List<Telerik.Sitefinity.Security.Model.Role>();
        }

        public override Telerik.Sitefinity.Security.Model.UserLink CreateUserLink()
        {
            return this.CreateUserLink(GetNewGuid());
        }

        public override void AddUserToRole(Telerik.Sitefinity.Security.Model.User user, Telerik.Sitefinity.Security.Model.Role role)
        {
            var link = this.CreateUserLink();
            link.UserId = user.Id;

            link.MembershipManagerInfo = this.UserManagerInfo;

            var scope = this.GetContext();
            Telerik.Sitefinity.Security.Model.Role existingRole = null;

            // Try get existing role - if a user is already added in this role, the role will exist in context of Sitefinity
            // When first time assigning the role this will throw exception - Item you are trying to access, no longer exists.
            try
            {
                existingRole = scope.GetItemById<Telerik.Sitefinity.Security.Model.Role>(role.Id.ToString());
            }
            catch (Exception ex)
            {
            }

            if (existingRole != null)
            {
                link.Role = existingRole;   
            }
            else
            {
                link.Role = role;  
            }

            scope.Add(link);
        }

        public override Telerik.Sitefinity.Security.Model.UserLink GetUserLink(Guid Id)
        {
            var query = SitefinityQuery.Get<UserLink>(this);
            var userLink = query.Where(ul => ul.Id == Id).FirstOrDefault();

                userLink.MembershipManagerInfo = this.UserManagerInfo;
                userLink.ApplicationName = this.ApplicationName;

            return userLink;           
        }

        public override IQueryable<Telerik.Sitefinity.Security.Model.UserLink> GetUserLinks()
        {
            var context = this.GetContext();
            var query = context.GetAll<UserLink>().Where(r => r.ApplicationName == this.ApplicationName).ToList()
                        .Select(rr => new UserLink()
                        {
                            Id = rr.Id,
                            Role = rr.Role,
                            UserId = rr.UserId,
                            MembershipManagerInfo = this.UserManagerInfo,
                            ApplicationName = this.ApplicationName
                        });

            return query.AsQueryable();       
        }

        public override void Delete(Telerik.Sitefinity.Security.Model.UserLink item)
        {

            var scope = this.GetContext();
            // associate with context
            var userLink = SitefinityQuery.Get<UserLink>(this).Where(r => r.Id == item.Id).FirstOrDefault();
            if (userLink == null)
            {
                 throw new ArgumentNullException("item","User link not found");
            }

            scope.Remove(userLink);
        }

        /// <summary>
        /// Gets the provider abilities for the current principal. E.g. which operations are supported and allowed
        /// </summary>
        /// <value>The provider abilities.</value>
        public override ProviderAbilities Abilities
        {
            get
            {
                ProviderAbilities abilities = new ProviderAbilities();
                abilities.ProviderName = this.Name;
                abilities.ProviderType = this.GetType().FullName;
                abilities.AddAbility("GetRole", true, true);
                abilities.AddAbility("AddRole", true, true);
                abilities.AddAbility("AssingUserToRole", true, true);
                abilities.AddAbility("UnAssingUserFromRole", true, true);
                abilities.AddAbility("DeleteRole", true, true);
                return abilities;
            }
        }

        /// <summary>
        /// Gets the user manager info.
        /// </summary>
        /// <value>The user manager info.</value>
        public ManagerInfo UserManagerInfo
        {
            get
            {
                if (this.userManagerInfo == null)
                {
                    var existingManInfo = this.GetContext().GetAll<ManagerInfo>()
                        .Where(m => m.ApplicationName == this.ApplicationName && m.ProviderName == this.membershipProviderName)
                        .FirstOrDefault();

                    if (existingManInfo != null)
                    {
                        return existingManInfo;
                    }

                    this.userManagerInfo = new ManagerInfo()
                    {
                        ApplicationName = this.ApplicationName,
                        ManagerType = this.GetUsersManager(membershipProviderName).GetType().FullName,
                        ProviderName = membershipProviderName,
                        Id = Guid.NewGuid()
                    };
                }

                return this.userManagerInfo;
            }
        }

        private ManagerInfo userManagerInfo;
        private string membershipProviderName = "Default";

        public override string ApplicationName
        {
            get
            {
                return "CustomRoles";
            }
        }

        public override IList<Telerik.Sitefinity.Security.Model.User> GetUsersInRole(Guid roleId)
        {
            return base.GetUsersInRole(roleId);
        }

        public override IQueryable<Telerik.Sitefinity.Security.Model.Role> GetRolesForUser(Guid userId)
        {
            return base.GetRolesForUser(userId);
        }

        public new Guid GetNewGuid()
        {
            return Guid.NewGuid();
        }

        public override void CommitTransaction()
        {
            this.GetContext().SaveChanges();
        }

        public Telerik.OpenAccess.Metadata.MetadataSource GetMetaDataSource(Telerik.Sitefinity.Model.IDatabaseMappingContext context)
        {
            return new RoleMetadataSource(context);
        }

        /// <summary>
        /// Gets or sets the OpenAccess context. Alternative to Database.
        /// </summary>
        /// <value>The context.</value>
        public OpenAccessProviderContext Context
        {
            get;
            set;
        }
    }
}