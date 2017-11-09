using System;
using System.Runtime.Serialization;
using System.IO;
using System.Linq;
using DB_Project.DataBase.Models;

namespace ElFinder.DTO
{
    [DataContract]
    internal abstract class DTOBase
    {
        protected static readonly DateTime _unixOrigin = new DateTime(1970, 1, 1, 0, 0, 0);
        private static DB_Project.DataBase.FileManager db = new DB_Project.DataBase.FileManager();

        /// <summary>
        ///  Name of file/dir. Required
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; protected set; }

        /// <summary>
        ///  Hash of current file/dir path, first symbol must be letter, symbols before _underline_ - volume id, Required.
        /// </summary>
        [DataMember(Name = "hash")]
        public string Hash { get; protected set; }

        /// <summary>
        ///  mime type. Required.
        /// </summary>
        [DataMember(Name = "mime")]
        public string Mime { get; protected set; }

        /// <summary>
        /// file modification time in unix timestamp. Required.
        /// </summary>
        [DataMember(Name = "ts")]
        public long UnixTimeStamp { get; protected set; }

        /// <summary>
        ///  file size in bytes
        /// </summary>
        [DataMember(Name = "size")]
        public long Size { get; protected set; }

        /// <summary>
        ///  is readable
        /// </summary>
        [DataMember(Name = "read")]
        public byte Read { get; protected set; }

        /// <summary>
        /// is writable
        /// </summary>
        [DataMember(Name = "write")]
        public byte Write { get; protected set; }

        /// <summary>
        ///  is file locked. If locked that object cannot be deleted and renamed
        /// </summary>
        [DataMember(Name = "locked")]
        public byte Locked { get; protected set; }

        public static DTOBase Create(FileInfo info, Root root)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            if (root == null)
                throw new ArgumentNullException("root");
            string parentPath = info.Directory.FullName.Substring(root.Directory.FullName.Length);
            string relativePath = info.FullName.Substring(root.Directory.FullName.Length);
            FileDTO response;

            var Files = db.Contents.Where(f => f.Type == DB_Project.DataBase.Models.Type.File);
            var UserId = int.Parse(System.Web.HttpContext.Current.Session["UserId"].ToString());
            var CurrentUser = db.Users.SingleOrDefault(u => u.ID == UserId);
            var RealPath = info.FullName.Replace(System.Web.HttpContext.Current.Server.MapPath("~/Content/Files"), "~");
            var CurrentFile = Files.SingleOrDefault(f => f.Name == info.Name && f.Path == RealPath && f.Type == DB_Project.DataBase.Models.Type.File);
            DB_Project.DataBase.Models.UserContentPermission UserReadOnlyPermission = null;
            DB_Project.DataBase.Models.RoleContentPermission RoleReadOnlyPermission = null;
            DB_Project.DataBase.Models.UserContentPermission UserLockedPermission = null;
            DB_Project.DataBase.Models.RoleContentPermission RoleLockedPermission = null;
            if (CurrentFile != null)
            {
                if (db.UserContentPermissions.Where(r => r.UserID == UserId).Count() > 0)
                {
                    var CurrentUserRoleID = CurrentUser.RoleId;
                    UserLockedPermission = db.UserContentPermissions.SingleOrDefault(v => v.UserID == UserId && v.ContentID == CurrentFile.ID && v.PermissionID == (int)Permissions.Locked);
                    UserReadOnlyPermission = db.UserContentPermissions.SingleOrDefault(v => v.UserID == UserId && v.ContentID == CurrentFile.ID && v.PermissionID == (int)Permissions.ReadOnly);
                    RoleLockedPermission = null;//db.RoleContentPermissions.SingleOrDefault(v => CurrentUserRoleID == v.RoleID && v.ContentID == CurrentFile.ID && v.PermissionID == (int)Permissions.Locked);
                    RoleReadOnlyPermission = null;//db.RoleContentPermissions.SingleOrDefault(v => CurrentUserRoleID == (v.RoleID) && v.ContentID == CurrentFile.ID && v.PermissionID == (int)Permissions.ReadOnly);
                }
                else
                {
                    var CurrentUserRoleID = CurrentUser.RoleId;
                    UserLockedPermission = db.UserContentPermissions.SingleOrDefault(v => v.UserID == UserId && v.ContentID == CurrentFile.ID && v.PermissionID == (int)Permissions.Locked);
                    UserReadOnlyPermission = db.UserContentPermissions.SingleOrDefault(v => v.UserID == UserId && v.ContentID == CurrentFile.ID && v.PermissionID == (int)Permissions.ReadOnly);
                    RoleLockedPermission = db.RoleContentPermissions.SingleOrDefault(v => CurrentUserRoleID == v.RoleID && v.ContentID == CurrentFile.ID && v.PermissionID == (int)Permissions.Locked);
                    RoleReadOnlyPermission = db.RoleContentPermissions.SingleOrDefault(v => CurrentUserRoleID == (v.RoleID) && v.ContentID == CurrentFile.ID && v.PermissionID == (int)Permissions.ReadOnly);
                }
            }

            if (root.CanCreateThumbnail(info))
            {
                ImageDTO imageResponse = new ImageDTO();
                imageResponse.Thumbnail = root.GetExistingThumbHash(info) ?? (object)1;
                var dim = root.GetImageDimension(info);
                imageResponse.Dimension = string.Format("{0}x{1}", dim.Width, dim.Height);
                response = imageResponse;
            }
            else
            {
                response = new FileDTO();
            }
            response.Read = 1;
            response.Write = (info.IsReadOnly || ((UserReadOnlyPermission != null) || (RoleReadOnlyPermission != null || UserReadOnlyPermission != null))) ? (byte)0 : (byte)1;
            if ((UserLockedPermission != null) || (RoleLockedPermission != null || UserLockedPermission != null))
            {
                response.Locked = (byte)1;
            }
            else
            {
                response.Locked = ((root.LockedFolders == null) ? (byte)0 : ((root.LockedFolders.Any(f => f == info.Directory.Name) || root.IsLocked) ? (byte)1 : (byte)0));
            }
            response.Name = info.Name;
            response.Size = info.Length;
            response.UnixTimeStamp = (long)(info.LastWriteTimeUtc - _unixOrigin).TotalSeconds;
            response.Mime = Helper.GetMimeType(info);
            response.Hash = root.VolumeId + Helper.EncodePath(relativePath);
            response.ParentHash = root.VolumeId + Helper.EncodePath(parentPath.Length > 0 ? parentPath : info.Directory.Name);
            return response;
        }

        public static DTOBase Create(DirectoryInfo directory, Root root)
        {
            if (directory == null)
                throw new ArgumentNullException("directory");
            if (root == null)
                throw new ArgumentNullException("root");

            var Folders = db.Contents.Where(f => f.Type == DB_Project.DataBase.Models.Type.Folder);
            var UserId = int.Parse(System.Web.HttpContext.Current.Session["UserId"].ToString());

            if (root.Directory.FullName == directory.FullName)
            {
                bool hasSubdirs = false;
                DirectoryInfo[] subdirs = directory.GetDirectories();

                foreach (var item in subdirs)
                {
                    var RealPath = item.FullName.Replace(System.Web.HttpContext.Current.Server.MapPath("~/Content/Files"), "~");
                    var CurrentFolder = Folders.SingleOrDefault(f => f.Name == item.Name && f.Path == RealPath && f.Type == DB_Project.DataBase.Models.Type.Folder);
                    var CurrentUser = db.Users.SingleOrDefault(u => u.ID == UserId);
                    var CurrentUserRoleID = CurrentUser.RoleId;
                    DB_Project.DataBase.Models.UserContentPermission UserHiddenPermission = null;
                    DB_Project.DataBase.Models.RoleContentPermission RoleHiddenPermission = null;
                    if (CurrentFolder != null)
                    {
                        if (db.UserContentPermissions.Where(r => r.UserID == UserId).Count() > 0)
                        {
                            UserHiddenPermission = db.UserContentPermissions.SingleOrDefault(v => v.UserID == UserId && v.ContentID == CurrentFolder.ID && v.PermissionID == (int)Permissions.Hidden);
                            RoleHiddenPermission = null;//db.RoleContentPermissions.SingleOrDefault(v => UserRolesID == (v.RoleID) && v.ContentID == CurrentFile.ID && v.PermissionID == (int)Permissions.Hidden);
                        }
                        else
                        {
                            UserHiddenPermission = db.UserContentPermissions.SingleOrDefault(v => v.UserID == UserId && v.ContentID == CurrentFolder.ID && v.PermissionID == (int)Permissions.Hidden);
                            RoleHiddenPermission = db.RoleContentPermissions.SingleOrDefault(v => CurrentUserRoleID == (v.RoleID) && v.ContentID == CurrentFolder.ID && v.PermissionID == (int)Permissions.Hidden);
                        }
                    }

                    if (((UserHiddenPermission == null) && (RoleHiddenPermission == null && RoleHiddenPermission == null)))
                    {
                        if ((item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                        {
                            hasSubdirs = true;
                            break;
                        }
                    }

                }
                RootDTO response = new RootDTO()
                {
                    Mime = "directory",
                    Dirs = hasSubdirs ? (byte)1 : (byte)0,
                    Hash = root.VolumeId + Helper.EncodePath(directory.Name),
                    Read = 1,
                    Write = root.IsReadOnly ? (byte)0 : (byte)1,
                    Locked = root.IsLocked ? (byte)1 : (byte)0,
                    Name = root.Alias,
                    Size = 0,
                    UnixTimeStamp = (long)(directory.LastWriteTimeUtc - _unixOrigin).TotalSeconds,
                    VolumeId = root.VolumeId
                };
                return response;
            }
            else
            {
                string parentPath = directory.Parent.FullName.Substring(root.Directory.FullName.Length);

                var RealPath = directory.FullName.Replace(System.Web.HttpContext.Current.Server.MapPath("~/Content/Files"), "~");
                var CurrentFolder = Folders.SingleOrDefault(f => f.Name == directory.Name && f.Path == RealPath && f.Type == DB_Project.DataBase.Models.Type.Folder);
                var CurrentUser = db.Users.SingleOrDefault(u => u.ID == UserId);
                DB_Project.DataBase.Models.UserContentPermission UserReadOnlyPermission = null;
                DB_Project.DataBase.Models.RoleContentPermission RoleReadOnlyPermission = null;
                DB_Project.DataBase.Models.UserContentPermission UserLockedPermission = null;
                DB_Project.DataBase.Models.RoleContentPermission RoleLockedPermission = null;
                if (CurrentFolder != null)
                {
                    if (db.UserContentPermissions.Where(r => r.UserID == UserId).Count() > 0)
                    {
                        var CurrentUserRoleID = CurrentUser.RoleId;
                        UserLockedPermission = db.UserContentPermissions.SingleOrDefault(v => v.UserID == UserId && v.ContentID == CurrentFolder.ID && v.PermissionID == (int)Permissions.Locked);
                        UserReadOnlyPermission = db.UserContentPermissions.SingleOrDefault(v => v.UserID == UserId && v.ContentID == CurrentFolder.ID && v.PermissionID == (int)Permissions.ReadOnly);
                        RoleLockedPermission = null;//db.RoleContentPermissions.SingleOrDefault(v => CurrentUserRoleID == v.RoleID && v.ContentID == CurrentFolder.ID && v.PermissionID == (int)Permissions.Locked);
                        RoleReadOnlyPermission = null;//db.RoleContentPermissions.SingleOrDefault(v => CurrentUserRoleID == (v.RoleID) && v.ContentID == CurrentFolder.ID && v.PermissionID == (int)Permissions.ReadOnly);
                    }
                    else
                    {
                        var CurrentUserRoleID = CurrentUser.RoleId;
                        UserLockedPermission = db.UserContentPermissions.SingleOrDefault(v => v.UserID == UserId && v.ContentID == CurrentFolder.ID && v.PermissionID == (int)Permissions.Locked);
                        UserReadOnlyPermission = db.UserContentPermissions.SingleOrDefault(v => v.UserID == UserId && v.ContentID == CurrentFolder.ID && v.PermissionID == (int)Permissions.ReadOnly);
                        RoleLockedPermission = db.RoleContentPermissions.SingleOrDefault(v => CurrentUserRoleID == v.RoleID && v.ContentID == CurrentFolder.ID && v.PermissionID == (int)Permissions.Locked);
                        RoleReadOnlyPermission = db.RoleContentPermissions.SingleOrDefault(v => CurrentUserRoleID == (v.RoleID) && v.ContentID == CurrentFolder.ID && v.PermissionID == (int)Permissions.ReadOnly);
                    }
                    if (((UserLockedPermission != null) || (RoleLockedPermission != null || UserLockedPermission != null)))
                    {
                        root.LockedFolders = new System.Collections.Generic.List<string>();
                        root.LockedFolders.Add(CurrentFolder.Name);
                    }
                }

                DirectoryDTO response = new DirectoryDTO()
                {
                    Mime = "directory",
                    ContainsChildDirs = directory.GetDirectories().Length > 0 ? (byte)1 : (byte)0,
                    Hash = root.VolumeId + Helper.EncodePath(directory.FullName.Substring(root.Directory.FullName.Length)),
                    Read = 1,
                    Write = (root.IsReadOnly || ((UserReadOnlyPermission != null) || (RoleReadOnlyPermission != null || UserReadOnlyPermission != null))) ? (byte)0 : (byte)1,
                    Locked = (root.LockedFolders == null ? (byte)0 : ((root.LockedFolders.Any(f => f == directory.Name) || root.IsLocked || (((UserLockedPermission != null) || (RoleLockedPermission != null || UserLockedPermission != null)))) ? (byte)1 : (byte)0)),
                    Size = 0,
                    Name = directory.Name,
                    UnixTimeStamp = (long)(directory.LastWriteTimeUtc - _unixOrigin).TotalSeconds,
                    ParentHash = root.VolumeId + Helper.EncodePath(parentPath.Length > 0 ? parentPath : directory.Parent.Name)
                };
                return response;
            }
        }

    }
}