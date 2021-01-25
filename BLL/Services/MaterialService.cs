using System;
using System.Collections.Generic;
using System.Linq;
using Final_Task.Data.Models;
using Microsoft.Extensions.Configuration;
using Version = Final_Task.Data.Models.Version;
using Microsoft.AspNetCore.Http;
using System.IO;
using Final_Task.Repository;

namespace Final_Task.Services
{
    public class MaterialService : IMaterialService
    {
        private readonly IMaterialRepository _materialRepository;
        private readonly IVersionRepository _versionRepository;
        private readonly IConfiguration _config;


        public MaterialService(IMaterialRepository materialRepository, IVersionRepository versionRepository, IConfiguration config)
        {
            _materialRepository = materialRepository;
            _versionRepository = versionRepository;
            _config = config;
        }

        public Material AddMaterial(Material material, IFormFile file)
        {
            Version newVersion;
            if (material != null)
            {
                newVersion = new Version
                {
                    Material = material,
                    UploadTime = DateTime.Now,
                    Size = file.Length,
                    VersionCounter = 1
                };
                string path = _config.GetValue<String>("FilesPath:Type:ProjectDirectory") + material.Name + "_v1";
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    file.CopyToAsync(fileStream);
                    fileStream.Flush();
                }
                _materialRepository.AddMaterial(material);
                _versionRepository.AddVersion(newVersion);
                _materialRepository.Save();
                return (material);
            }
            return null;
        }

        public Version AddVersion(string name, IFormFile file)
        {
            Material material = _materialRepository.GetMaterialByName(name);
            Version newVersion;
            if (material != null)
            {
                newVersion = new Version
                {
                    Material = material,
                    UploadTime = DateTime.Now,
                    Size = file.Length,
                    VersionCounter = material.Versions.Count() + 1
                };
                string path = _config.GetValue<String>("FilesPath:Type:ProjectDirectory") + material.Name + "_v" + (material.Versions.Count() + 1);
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    file.CopyToAsync(fileStream);
                    fileStream.Flush();
                }
                _versionRepository.AddVersion(newVersion);
                _versionRepository.Save();
                return (newVersion);
            }
            return null;
        }

        public Material ChangeCategory(string name, string category)
        {
            var material = _materialRepository.GetMaterialByName(name);
            if (material != null)
            {
                material.Category = category;
                _materialRepository.Save();
                return (material);
            }
            return null;
        }

        public Material GetMaterialByName(string name)
        {
            var material = _materialRepository.GetMaterialByName(name);
            if (material != null)
                return (material);
            return null;
        }

        public IList<Material> GetAllMaterials(string category)
        {
            var material = _materialRepository.GetMaterials();
            return (material);
        }

        public IList<Material> GetFilteredMaterials(string category)
        {
            var filteredMaterials = _materialRepository.GetFilteredMaterial(category);
            if (filteredMaterials != null)
                return (filteredMaterials);
            return null;
        }

        public byte[] DownloadMaterial(string name, int? version)
        {
            string path;
            byte[] mas;
            var material = _materialRepository.GetMaterialByName(name);
            if (material != null)
            {
                if (version != null)
                    path = _config.GetValue<String>("FilesPath:Type:ProjectDirectory") + material.Name + "_v" + version;
                else
                    path = _config.GetValue<String>("FilesPath:Type:ProjectDirectory") + material.Name + "_v" + material.Versions.Count();
                mas = File.ReadAllBytes(path);
                return (mas);
            }
            return null;
        }
    }
}
