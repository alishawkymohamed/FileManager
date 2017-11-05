using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ElFinder.DTO;
using ElFinder.Response;
using DB_Project.DataBase.Models;

namespace ElFinder
{
    /// <summary>
    /// Represents a driver for local file system
    /// </summary>
    public class FileSystemDriver : IDriver
    {
        #region private  
        private const string _volumePrefix = "v";
        private List<Root> _roots;
        private DB_Project.DataBase.FileManager db;



        private JsonResult Json(object data)
        {
            return new JsonDataContractResult(data) { JsonRequestBehavior = JsonRequestBehavior.AllowGet, ContentType = "text/html" };
        }
        private void DirectoryCopy(DirectoryInfo sourceDir, string destDirName, bool copySubDirs)
        {
            DirectoryInfo[] dirs = sourceDir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!sourceDir.Exists)
            {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDir.FullName);
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the file contents of the directory to copy.
            FileInfo[] files = sourceDir.GetFiles();

            foreach (FileInfo file in files)
            {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine(destDirName, file.Name);
                db.Contents.Add(new DB_Project.DataBase.Models.Content() { Name = file.Name, Path = temppath, Type = DB_Project.DataBase.Models.Type.File });
                file.CopyTo(temppath, false);
            }
            // If copySubDirs is true, copy the subdirectories.

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    // Create the subdirectory.
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    db.Contents.Add(new DB_Project.DataBase.Models.Content() { Name = subdir.Name, Path = temppath.Replace(System.Web.HttpContext.Current.Server.MapPath("~/Content/Files"), "~"), Type = DB_Project.DataBase.Models.Type.Folder });
                    // Copy the subdirectories.
                    DirectoryCopy(subdir, temppath, copySubDirs);
                }
            }
            db.SaveChanges();
        }
        private void DirectoryCut(DirectoryInfo sourceDir, string destDirName, bool cutSubDirs)
        {
            DirectoryInfo[] dirs = sourceDir.GetDirectories();

            FileInfo[] files = sourceDir.GetFiles();

            foreach (FileInfo file in files)
            {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine(destDirName, file.Name);
                db.Contents.Where(f => f.Type == DB_Project.DataBase.Models.Type.File)
                           .SingleOrDefault(f => f.Path == file.FullName.Replace(System.Web.HttpContext.Current.Server.MapPath("~/Content/Files"), "~")).Path = temppath.Replace(System.Web.HttpContext.Current.Server.MapPath("~/Content/Files"), "~");
            }
            // If cutSubDirs is true, cut the subdirectories.

            if (cutSubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    // Create the subdirectory.
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    db.Contents.Where(f => f.Type == DB_Project.DataBase.Models.Type.Folder)
                               .SingleOrDefault(f => f.Path == subdir.FullName.Replace(System.Web.HttpContext.Current.Server.MapPath("~/Content/Files"), "~")).Path = temppath;
                    // cut the subdirectories.
                    DirectoryCut(subdir, temppath, cutSubDirs);
                }
            }
            db.SaveChanges();
        }
        private void DirectoryDelete(DirectoryInfo sourceDir, bool DeleteSubDirs)
        {
            DirectoryInfo[] dirs = sourceDir.GetDirectories();

            FileInfo[] files = sourceDir.GetFiles();

            foreach (FileInfo file in files)
            {
                // Delete the path of the file from Database ...
                var tempFile = db.Contents.Where(f => f.Type == DB_Project.DataBase.Models.Type.File)
                           .SingleOrDefault(f => f.Path == file.FullName.Replace(System.Web.HttpContext.Current.Server.MapPath("~/Content/Files"), "~"));
                db.Contents.Remove(tempFile);
            }
            // If DeleteSubDirs is true, Delete the subdirectories.

            if (DeleteSubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    // Delete the path of the SubFolders from Database ...
                    var tempFile = db.Contents.Where(f => f.Type == DB_Project.DataBase.Models.Type.Folder)
                               .SingleOrDefault(f => f.Path == subdir.FullName.Replace(System.Web.HttpContext.Current.Server.MapPath("~/Content/Files"), "~"));
                    db.Contents.Remove(tempFile);
                    // Delete the subdirectories.
                    DirectoryDelete(subdir, DeleteSubDirs);
                }
            }
            db.SaveChanges();
        }
        private void RemoveThumbs(FullPath path)
        {
            if (path.Directory != null)
            {
                string thumbPath = path.Root.GetExistingThumbPath(path.Directory);
                if (thumbPath != null)
                    Directory.Delete(thumbPath, true);
            }
            else
            {
                string thumbPath = path.Root.GetExistingThumbPath(path.File);
                if (thumbPath != null)
                    File.Delete(thumbPath);
            }
        }
        #endregion

        #region public 

        public FullPath ParsePath(string target)
        {
            string volumePrefix = null;
            string pathHash = null;
            for (int i = 0; i < target.Length; i++)
            {
                if (target[i] == '_')
                {
                    pathHash = target.Substring(i + 1);
                    volumePrefix = target.Substring(0, i + 1);
                    break;
                }
            }
            Root root = _roots.First(r => r.VolumeId == volumePrefix);
            string path = Helper.DecodePath(pathHash);
            string dirUrl = path != root.Directory.Name ? path : string.Empty;
            var dir = new DirectoryInfo(root.Directory.FullName + dirUrl);
            if (dir.Exists)
            {
                return new FullPath(root, dir);
            }
            else
            {
                var file = new FileInfo(root.Directory.FullName + dirUrl);
                return new FullPath(root, file);
            }
        }

        /// <summary>
        /// Initialize new instance of class ElFinder.FileSystemDriver 
        /// </summary>
        public FileSystemDriver()
        {
            _roots = new List<Root>();
            db = new DB_Project.DataBase.FileManager();
        }

        /// <summary>
        /// Adds an object to the end of the roots.
        /// </summary>
        /// <param name="item"></param>
        public void AddRoot(Root item)
        {
            _roots.Add(item);
            item.VolumeId = _volumePrefix + _roots.Count + "_";
        }

        /// <summary>
        /// Gets collection of roots
        /// </summary>
        public IEnumerable<Root> Roots { get { return _roots; } }
        #endregion public

        #region   IDriver
        JsonResult IDriver.Open(string target, bool tree)
        {
            FullPath fullPath = ParsePath(target);
            OpenResponse answer = new OpenResponse(DTOBase.Create(fullPath.Directory, fullPath.Root), fullPath);
            foreach (FileInfo item in fullPath.Directory.GetFiles())
            {
                if ((item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                    answer.Files.Add(DTOBase.Create(item, fullPath.Root));
            }
            foreach (DirectoryInfo item in fullPath.Directory.GetDirectories())
            {
                if ((item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                    answer.Files.Add(DTOBase.Create(item, fullPath.Root));
            }
            return Json(answer);
        }
        JsonResult IDriver.Init(string target)
        {
            FullPath fullPath;
            if (string.IsNullOrEmpty(target))
            {
                Root root = _roots.FirstOrDefault(r => r.StartPath != null);
                if (root == null)
                    root = _roots.First();
                fullPath = new FullPath(root, root.StartPath ?? root.Directory);
            }
            else
            {
                fullPath = ParsePath(target);
            }
            InitResponse answer = new InitResponse(DTOBase.Create(fullPath.Directory, fullPath.Root), new Options(fullPath));
            var Files = db.Contents.Where(f => f.Type == DB_Project.DataBase.Models.Type.File);
            var Folders = db.Contents.Where(f => f.Type == DB_Project.DataBase.Models.Type.Folder);
            var UserId = int.Parse(System.Web.HttpContext.Current.Session["UserId"].ToString());
            foreach (FileInfo item in fullPath.Directory.GetFiles())
            {
                var RealPath = item.FullName.Replace(System.Web.HttpContext.Current.Server.MapPath("~/Content/Files"), "~");
                var CurrentFile = Files.SingleOrDefault(f => f.Name == item.Name && f.Path == RealPath && f.Type == DB_Project.DataBase.Models.Type.File);
                DB_Project.DataBase.Models.UserContentPermission HiddenPermission = null;
                if (CurrentFile != null)
                {
                    HiddenPermission = db.UserContentPermissions.SingleOrDefault(v => v.UserID == UserId && v.ContentID == CurrentFile.ID && v.PermissionID == (int)Permissions.Hidden);
                }

                if (HiddenPermission == null)
                {
                    if ((item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                        answer.Files.Add(DTOBase.Create(item, fullPath.Root));
                }
            }
            foreach (DirectoryInfo item in fullPath.Directory.GetDirectories())
            {
                var RealPath = item.FullName.Replace(System.Web.HttpContext.Current.Server.MapPath("~/Content/Files"), "~");
                var CurrentFolder = Folders.SingleOrDefault(f => f.Name == item.Name && f.Path == RealPath && f.Type == DB_Project.DataBase.Models.Type.Folder);
                DB_Project.DataBase.Models.UserContentPermission HiddenPermission = null;
                if (CurrentFolder != null)
                {
                    HiddenPermission = db.UserContentPermissions.SingleOrDefault(v => v.UserID == UserId && v.ContentID == CurrentFolder.ID && v.PermissionID == (int)Permissions.Hidden);
                }
                if (HiddenPermission == null)
                {
                    if ((item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                        answer.Files.Add(DTOBase.Create(item, fullPath.Root));
                }
            }
            foreach (Root item in _roots)
            {
                answer.Files.Add(DTOBase.Create(item.Directory, item));
            }
            if (fullPath.Root.Directory.FullName != fullPath.Directory.FullName)
            {
                foreach (DirectoryInfo item in fullPath.Root.Directory.GetDirectories())
                {
                    if ((item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                        answer.Files.Add(DTOBase.Create(item, fullPath.Root));
                }
            }
            if (fullPath.Root.MaxUploadSize.HasValue)
            {
                answer.UploadMaxSize = fullPath.Root.MaxUploadSizeInKb.Value + "K";
            }
            return Json(answer);
        }
        ActionResult IDriver.File(string target, bool download)
        {
            FullPath fullPath = ParsePath(target);
            if (fullPath.IsDirectoty)
                return new HttpStatusCodeResult(403, "You can not download whole folder");
            if (!fullPath.File.Exists)
                return new HttpNotFoundResult("File not found");
            if (fullPath.Root.IsShowOnly)
                return new HttpStatusCodeResult(403, "Access denied. Volume is for show only");
            return new DownloadFileResult(fullPath.File, download);
        }
        JsonResult IDriver.Parents(string target)
        {
            FullPath fullPath = ParsePath(target);
            TreeResponse answer = new TreeResponse();
            if (fullPath.Directory.FullName == fullPath.Root.Directory.FullName)
            {
                answer.Tree.Add(DTOBase.Create(fullPath.Directory, fullPath.Root));
            }
            else
            {
                DirectoryInfo parent = fullPath.Directory;
                foreach (var item in parent.Parent.GetDirectories())
                {
                    answer.Tree.Add(DTOBase.Create(item, fullPath.Root));
                }
                while (parent.FullName != fullPath.Root.Directory.FullName)
                {
                    parent = parent.Parent;
                    answer.Tree.Add(DTOBase.Create(parent, fullPath.Root));
                }
            }
            return Json(answer);
        }
        JsonResult IDriver.Tree(string target)
        {
            FullPath fullPath = ParsePath(target);
            TreeResponse answer = new TreeResponse();
            foreach (var item in fullPath.Directory.GetDirectories())
            {
                if ((item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                    answer.Tree.Add(DTOBase.Create(item, fullPath.Root));
            }
            return Json(answer);
        }
        JsonResult IDriver.List(string target)
        {
            FullPath fullPath = ParsePath(target);
            ListResponse answer = new ListResponse();
            foreach (var item in fullPath.Directory.GetFileSystemInfos())
            {
                answer.List.Add(item.Name);
            }
            return Json(answer);
        }
        JsonResult IDriver.MakeDir(string target, string name)
        {
            FullPath fullPath = ParsePath(target);
            DirectoryInfo newDir = Directory.CreateDirectory(Path.Combine(fullPath.Directory.FullName, name));
            // Save in Database ....
            db.Contents.Add(new DB_Project.DataBase.Models.Content() { Name = name, Path = newDir.FullName.Replace(System.Web.HttpContext.Current.Server.MapPath("~/Content/Files"), "~"), Type = DB_Project.DataBase.Models.Type.Folder });
            try
            {
                db.SaveChanges();
            }
            catch (Exception)
            {
                return Error.MissedParameter("Error Occured .. Reload and Try Again Later");
            }
            return Json(new AddResponse(newDir, fullPath.Root));
        }
        JsonResult IDriver.MakeFile(string target, string name)
        {
            FullPath fullPath = ParsePath(target);
            FileInfo newFile = new FileInfo(Path.Combine(fullPath.Directory.FullName, name));
            db.Contents.Add(new DB_Project.DataBase.Models.Content() { Name = name, Path = newFile.FullName.Replace(System.Web.HttpContext.Current.Server.MapPath("~/Content/Files"), "~"), Type = DB_Project.DataBase.Models.Type.File });
            try
            {
                db.SaveChanges();
            }
            catch (Exception)
            {
                return Error.MissedParameter("Error Occured .. Reload and Try Again Later");
            }
            newFile.Create().Close();
            return Json(new AddResponse(newFile, fullPath.Root));
        }
        JsonResult IDriver.Rename(string target, string name)
        {
            FullPath fullPath = ParsePath(target);
            var answer = new ReplaceResponse();
            answer.Removed.Add(target);
            RemoveThumbs(fullPath);
            if (fullPath.Directory != null)
            {
                string newPath = Path.Combine(fullPath.Directory.Parent.FullName, name);
                var folder = db.Contents.SingleOrDefault(f => f.Path == fullPath.Directory.FullName.Replace(System.Web.HttpContext.Current.Server.MapPath("~/Content/Files"), "~"));
                folder.Path = newPath.Replace(System.Web.HttpContext.Current.Server.MapPath("~/Content/Files"), "~"); folder.Name = name;
                db.SaveChanges();
                System.IO.Directory.Move(fullPath.Directory.FullName, newPath);
                answer.Added.Add(DTOBase.Create(new DirectoryInfo(newPath), fullPath.Root));
            }
            else
            {
                string newPath = Path.Combine(fullPath.File.DirectoryName, name);
                var file = db.Contents.SingleOrDefault(f => f.Path == fullPath.Directory.FullName.Replace(System.Web.HttpContext.Current.Server.MapPath("~/Content/Files"), "~"));
                file.Path = newPath.Replace(System.Web.HttpContext.Current.Server.MapPath("~/Content/Files"), "~"); file.Name = name;
                File.Move(fullPath.File.FullName, newPath);
                answer.Added.Add(DTOBase.Create(new FileInfo(newPath), fullPath.Root));
            }
            return Json(answer);
        }
        JsonResult IDriver.Remove(IEnumerable<string> targets)
        {
            RemoveResponse answer = new RemoveResponse();
            foreach (string item in targets)
            {
                FullPath fullPath = ParsePath(item);
                RemoveThumbs(fullPath);
                if (fullPath.Directory != null)
                {
                    DirectoryDelete(fullPath.Directory, true);
                    System.IO.Directory.Delete(fullPath.Directory.FullName, true);
                }
                else
                {
                    File.Delete(fullPath.File.FullName);
                }
                answer.Removed.Add(item);
            }
            return Json(answer);
        }
        JsonResult IDriver.Get(string target)
        {
            FullPath fullPath = ParsePath(target);
            GetResponse answer = new GetResponse();
            using (StreamReader reader = new StreamReader(fullPath.File.OpenRead()))
            {
                answer.Content = reader.ReadToEnd();
            }
            return Json(answer);
        }
        JsonResult IDriver.Put(string target, string content)
        {
            FullPath fullPath = ParsePath(target);
            ChangedResponse answer = new ChangedResponse();
            using (StreamWriter writer = new StreamWriter(fullPath.File.FullName, false))
            {
                writer.Write(content);
            }
            answer.Changed.Add((FileDTO)DTOBase.Create(fullPath.File, fullPath.Root));
            return Json(answer);
        }
        JsonResult IDriver.Paste(string source, string dest, IEnumerable<string> targets, bool isCut)
        {
            FullPath destPath = ParsePath(dest);
            FullPath OldPath = ParsePath(source);
            ReplaceResponse response = new ReplaceResponse();
            foreach (var item in targets)
            {
                FullPath src = ParsePath(item);
                if (src.Directory != null)
                {
                    DirectoryInfo newDir = new DirectoryInfo(Path.Combine(destPath.Directory.FullName, src.Directory.Name));
                    if (newDir.Exists)
                        Directory.Delete(newDir.FullName, true);
                    if (isCut)
                    {
                        DirectoryCut(src.Directory, newDir.FullName, true);
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            return Error.MissedParameter("Error Occured .. Reload and Try Again Later");
                        }
                        var OldFolderDB = db.Contents.SingleOrDefault(f => f.Path == OldPath.Directory.FullName.Replace(System.Web.HttpContext.Current.Server.MapPath("~/Content/Files"), "~") && f.Type == DB_Project.DataBase.Models.Type.Folder);
                        OldFolderDB.Path = newDir.FullName;
                        db.SaveChanges();
                        RemoveThumbs(src);
                        src.Directory.MoveTo(newDir.FullName);
                        response.Removed.Add(item);
                    }
                    else
                    {
                        DirectoryCopy(src.Directory, newDir.FullName, true);
                    }
                    response.Added.Add(DTOBase.Create(newDir, destPath.Root));
                }
                else
                {
                    string newFilePath = Path.Combine(destPath.Directory.FullName, src.File.Name);
                    if (File.Exists(newFilePath))
                        File.Delete(newFilePath);
                    if (isCut)
                    {
                        var OldPathDB = db.Contents.SingleOrDefault(c => c.Path == OldPath.File.FullName.Replace(System.Web.HttpContext.Current.Server.MapPath("~/Content/Files"), "~") && c.Type == DB_Project.DataBase.Models.Type.File);
                        OldPathDB.Path = newFilePath;
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            return Error.MissedParameter("Error Occured .. Reload and Try Again Later");
                        }
                        RemoveThumbs(src);
                        src.File.MoveTo(newFilePath);
                        response.Removed.Add(item);
                    }
                    else
                    {
                        db.Contents.Add(new DB_Project.DataBase.Models.Content() { Type = DB_Project.DataBase.Models.Type.File, Path = newFilePath.Replace(System.Web.HttpContext.Current.Server.MapPath("~/Content/Files"), "~"), Name = src.File.Name });
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            return Error.MissedParameter("Error Occured .. Reload and Try Again Later");
                        }
                        File.Copy(src.File.FullName, newFilePath);
                    }
                    response.Added.Add(DTOBase.Create(new FileInfo(newFilePath), destPath.Root));
                }
            }
            return Json(response);
        }
        JsonResult IDriver.Upload(string target, System.Web.HttpFileCollectionBase targets)
        {
            FullPath dest = ParsePath(target);
            var response = new AddResponse();
            if (dest.Root.MaxUploadSize.HasValue)
            {
                for (int i = 0; i < targets.AllKeys.Length; i++)
                {
                    HttpPostedFileBase file = targets[i];
                    if (file.ContentLength > dest.Root.MaxUploadSize.Value)
                    {
                        return Error.MaxUploadFileSize();
                    }
                }
            }
            for (int i = 0; i < targets.AllKeys.Length; i++)
            {
                HttpPostedFileBase file = targets[i];
                FileInfo path = new FileInfo(Path.Combine(dest.Directory.FullName, Path.GetFileName(file.FileName)));

                if (path.Exists)
                {
                    if (dest.Root.UploadOverwrite)
                    {
                        //if file already exist we rename the current file, 
                        //and if upload is succesfully delete temp file, in otherwise we restore old file
                        string tmpPath = path.FullName + Guid.NewGuid();
                        bool uploaded = false;
                        try
                        {
                            file.SaveAs(tmpPath);
                            uploaded = true;
                        }
                        catch { }
                        finally
                        {
                            if (uploaded)
                            {
                                File.Delete(path.FullName);
                                File.Move(tmpPath, path.FullName);
                            }
                            else
                            {
                                File.Delete(tmpPath);
                            }
                        }
                    }
                    else
                    {
                        file.SaveAs(Path.Combine(path.DirectoryName, Helper.GetDuplicatedName(path)));
                    }
                }
                else
                {
                    db.Contents.Add(new DB_Project.DataBase.Models.Content() { Name = path.Name, Path = path.FullName.Replace(System.Web.HttpContext.Current.Server.MapPath("~/Content/Files"), "~") });
                    db.SaveChanges();
                    file.SaveAs(path.FullName);
                }
                response.Added.Add((FileDTO)DTOBase.Create(new FileInfo(path.FullName), dest.Root));
            }
            return Json(response);
        }
        JsonResult IDriver.Duplicate(IEnumerable<string> targets)
        {
            AddResponse response = new AddResponse();
            foreach (var target in targets)
            {
                FullPath fullPath = ParsePath(target);
                if (fullPath.Directory != null)
                {
                    var parentPath = fullPath.Directory.Parent.FullName;
                    var name = fullPath.Directory.Name;
                    var newName = string.Format(@"{0}\{1} copy", parentPath, name);
                    var FolderName = string.Format(@"{0} copy", name);

                    if (!Directory.Exists(newName))
                    {
                        db.Contents.Add(new DB_Project.DataBase.Models.Content() { Name = FolderName, Path = newName.Replace(System.Web.HttpContext.Current.Server.MapPath("~/Content/Files"), "~"), Type = DB_Project.DataBase.Models.Type.Folder });
                        db.SaveChanges();
                        DirectoryCopy(fullPath.Directory, newName, true);
                    }
                    else
                    {
                        for (int i = 1; i < 100; i++)
                        {
                            newName = string.Format(@"{0}\{1} copy {2}", parentPath, name, i);
                            FolderName = string.Format(@"{0} copy {1}", name, i);

                            if (!Directory.Exists(newName))
                            {
                                db.Contents.Add(new DB_Project.DataBase.Models.Content() { Name = FolderName, Path = newName.Replace(System.Web.HttpContext.Current.Server.MapPath("~/Content/Files"), "~"), Type = DB_Project.DataBase.Models.Type.Folder });
                                db.SaveChanges();
                                DirectoryCopy(fullPath.Directory, newName, true);
                                break;
                            }
                        }
                    }
                    response.Added.Add(DTOBase.Create(new DirectoryInfo(newName), fullPath.Root));
                }
                else
                {
                    var parentPath = fullPath.File.Directory.FullName;
                    var name = fullPath.File.Name.Substring(0, fullPath.File.Name.Length - fullPath.File.Extension.Length);
                    var ext = fullPath.File.Extension;
                    var newName = string.Format(@"{0}\{1} copy{2}", parentPath, name, ext);
                    var fileName = string.Format(@"{0} copy{1}", name, ext);
                    if (!File.Exists(newName))
                    {
                        db.Contents.Add(new DB_Project.DataBase.Models.Content() { Name = fileName, Path = newName.Replace(System.Web.HttpContext.Current.Server.MapPath("~/Content/Files"), "~"), Type = DB_Project.DataBase.Models.Type.File });
                        db.SaveChanges();
                        fullPath.File.CopyTo(newName);
                    }
                    else
                    {
                        for (int i = 1; i < 100; i++)
                        {
                            newName = string.Format(@"{0}\{1} copy {2}{3}", parentPath, name, i, ext);
                            fileName = string.Format(@"{0} copy {1}{2}", name, i, ext);
                            if (!File.Exists(newName))
                            {
                                db.Contents.Add(new DB_Project.DataBase.Models.Content() { Name = fileName, Path = newName.Replace(System.Web.HttpContext.Current.Server.MapPath("~/Content/Files"), "~"), Type = DB_Project.DataBase.Models.Type.File });
                                db.SaveChanges();
                                fullPath.File.CopyTo(newName);
                                break;
                            }
                        }
                    }
                    response.Added.Add(DTOBase.Create(new FileInfo(newName), fullPath.Root));
                }
            }
            return Json(response);
        }
        JsonResult IDriver.Thumbs(IEnumerable<string> targets)
        {
            ThumbsResponse response = new ThumbsResponse();
            foreach (string target in targets)
            {
                FullPath path = ParsePath(target);
                response.Images.Add(target, path.Root.GenerateThumbHash(path.File));
            }
            return Json(response);
        }
        JsonResult IDriver.Dim(string target)
        {
            FullPath path = ParsePath(target);
            DimResponse response = new DimResponse(path.Root.GetImageDimension(path.File));
            return Json(response);
        }
        JsonResult IDriver.Resize(string target, int width, int height)
        {
            FullPath path = ParsePath(target);
            RemoveThumbs(path);
            path.Root.PicturesEditor.Resize(path.File.FullName, width, height);
            var output = new ChangedResponse();
            output.Changed.Add((FileDTO)DTOBase.Create(path.File, path.Root));
            return Json(output);
        }
        JsonResult IDriver.Crop(string target, int x, int y, int width, int height)
        {
            FullPath path = ParsePath(target);
            RemoveThumbs(path);
            path.Root.PicturesEditor.Crop(path.File.FullName, x, y, width, height);
            var output = new ChangedResponse();
            output.Changed.Add((FileDTO)DTOBase.Create(path.File, path.Root));
            return Json(output);
        }
        JsonResult IDriver.Rotate(string target, int degree)
        {
            FullPath path = ParsePath(target);
            RemoveThumbs(path);
            path.Root.PicturesEditor.Rotate(path.File.FullName, degree);
            var output = new ChangedResponse();
            output.Changed.Add((FileDTO)DTOBase.Create(path.File, path.Root));
            return Json(output);
        }
        [NonAction]
        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
        }

        #endregion IDriver
    }
}