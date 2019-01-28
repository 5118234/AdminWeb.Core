using AdminWeb.Core.Services.BASE;
using AdminWeb.Core.Model.Models;
using AdminWeb.Core.IRepository;
using AdminWeb.Core.IServices;
using AutoMapper;
using AdminWeb.Core.Model.ViewModels;
using AdminWeb.Core.Model;
using System.Threading.Tasks;
using System.Collections.Generic;
using SqlSugar;

namespace AdminWeb.Core.Services
{	
	/// <summary>
	/// ModuleServices
	/// </summary>	
	public class ModuleServices : BaseServices<Module>, IModuleServices
    {
	
        IModuleRepository dal;
        IMapper IMapper;
        public ModuleServices(IModuleRepository dal, IMapper IMapper)
        {
            this.dal = dal;
            base.baseDal = dal;
            this.IMapper = IMapper;
        }


        /// <summary>
        /// ��ȡ�˵���ҳ
        /// </summary>
        /// <param name="moduleViewModels"></param>
        /// <returns></returns>
        public List<ModuleViewModels> ListPageModules(ModuleViewModels moduleViewModels)
        {
            List<ModuleViewModels> viewModels = new List<ModuleViewModels>();

            var total = moduleViewModels.TotalCount;
            var orderByFileds = !string.IsNullOrEmpty(moduleViewModels.OrderByFileds) ? "" : moduleViewModels.OrderByFileds;

            //��̬ƴ����ķ��
            var query = Expressionable.Create<Module>().AndIF(!string.IsNullOrEmpty(moduleViewModels.Name), s => s.Name == moduleViewModels.Name).AndIF(!string.IsNullOrEmpty(moduleViewModels.Action), s => s.Action == moduleViewModels.Action).ToExpression();


            var models = dal.Query(query, moduleViewModels.PageIndex, moduleViewModels.PageSize, moduleViewModels.OrderByFileds, ref total);

            //var models2 = dal.GetSimpleClient()
            //                .Queryable<Module, ModulePermission>((ml, mp) => new object[] { JoinType.Left, ml.Id == mp.ModuleId })
            //                .WhereIF(!string.IsNullOrEmpty(moduleViewModels.Name), (ml, mp) => ml.Name == moduleViewModels.Name)
            //                .Select((ml, mp) =>new ModulePermission {CreateBy=mp.CreateBy,CreateTime=mp.CreateTime })
            //                .ToPageList(moduleViewModels.PageIndex, moduleViewModels.PageSize, ref total);

            foreach (var s in models)
            {
                viewModels.Add(IMapper.Map<ModuleViewModels>(s));
            }
            return viewModels;
        }

        /// <summary>
        /// ��ȡ�����˵���Ϣ
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<ModuleViewModels> GetModule(int Id)
        {
            var module =await QueryByID(Id);
            return IMapper.Map<ModuleViewModels>(module);
        }

        /// <summary>
        /// ��ӵ����˵�
        /// </summary>
        /// <param name="moduleViewModels"></param>
        /// <returns></returns>

        public async Task<bool> AddModule(ModuleViewModels moduleViewModels)
        {
            //ת��model
            var module = IMapper.Map<Module>(moduleViewModels);
            return await Add(module)>0; 
        }


        /// <summary>
        /// ����Model
        /// </summary>
        /// <param name="moduleViewModels"></param>
        /// <returns></returns>
        public async Task<bool> UpdateModule(ModuleViewModels moduleViewModels)
        {
            //ת��model
            var module = IMapper.Map<Module>(moduleViewModels);
            //���Ը��µ��ֶ�
            List<string> lstIgnoreColumns = new List<string>()
            {
                "ParentId","CreateId","CreateBy","CreateTime"
            };
            return await Update(module,null, lstIgnoreColumns,"");
        }

        /// <summary>
        /// ɾ���˵�
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public Task<bool> DeleteModule(int Id)
        {
            return DeleteById(Id);
        }

    }
}
