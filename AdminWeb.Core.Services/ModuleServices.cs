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
using AdminWeb.Core.FrameWork.IRepository;

namespace AdminWeb.Core.Services
{
    /// <summary>
    /// ModuleServices
    /// </summary>	
    public class ModuleServices : BaseServices<Module>, IModuleServices
    {

        IModuleRepository dal;
        IUserRoleServices userRoleServices;
        IMapper IMapper;
        List<string> UserRoles;
        public ModuleServices(IModuleRepository dal, IMapper IMapper, IUserRoleServices userRoleServices)
        {
            this.dal = dal;
            base.baseDal = dal;
            this.IMapper = IMapper;
            this.userRoleServices = userRoleServices;
        }


        /// <summary>
        /// ��ȡ�˵���ҳ
        /// </summary>
        /// <param name="moduleViewModels"></param>
        /// <returns></returns>
        public TableModel<ModuleViewModels> ListPageModules(ModuleViewModels moduleViewModels)
        {
            List<ModuleViewModels> viewModels = new List<ModuleViewModels>();

            var total = moduleViewModels.TotalCount;
            var orderByFileds = !string.IsNullOrEmpty(moduleViewModels.OrderByFileds) ? "" : moduleViewModels.OrderByFileds;

            //��̬ƴ����ķ��
            var query = Expressionable.Create<Module>().AndIF(!string.IsNullOrEmpty(moduleViewModels.Name), s => s.Name == moduleViewModels.Name).ToExpression();


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

            return new TableModel<ModuleViewModels>() { Code = 1, Count = total, Data = viewModels, Msg = "success" };
        }

        /// <summary>
        /// ��ȡ�����˵���Ϣ
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<ModuleViewModels> GetModule(int Id)
        {
            var module = await QueryByID(Id);
            var moduleViewModel = IMapper.Map<ModuleViewModels>(module);

            moduleViewModel.Meta.Icon = moduleViewModel.Icon;
            moduleViewModel.Meta.Title = moduleViewModel.Title;
            moduleViewModel.Meta.Role = userRoleServices.ListUserRoles(Id);

            return moduleViewModel;
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
            return await Add(module) > 0;
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
                "CreateId","CreateBy","CreateTime"
            };
            return await Update(module, null, lstIgnoreColumns, "");
        }

        /// <summary>
        /// ɾ���˵�
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteModule(int Id)
        {
            return await DeleteById(Id);
        }

        /// <summary>
        /// ��ȡ��ǰ�˵���(�ͻ����ݹ鵱ǰ�˵�)
        /// </summary>
        /// <returns></returns>
        public List<ModuleViewModels> ListClientModules()
        {
            var modules = dal.GetSimpleClient().GetSimpleClient<Module>().GetList(s => s.IsDeleted == false);
            List<ModuleViewModels> viewModels = new List<ModuleViewModels>();
            foreach (var t in modules)
            {
                viewModels.Add(IMapper.Map<ModuleViewModels>(t));
            }
            foreach(var s in viewModels)
            {
                s.Meta.Role = new List<string>() { "Admin" };
            }
            return viewModels;
        }


        /// <summary>
        /// ��ȡ��ǰ�˵���(�������ݹ鵱ǰ�˵�)
        /// </summary>
        /// <returns></returns>
        public List<ModuleViewModels> ListServerModules()
        {
            var modules = dal.GetSimpleClient().GetSimpleClient<Module>().GetList(s => s.IsDeleted == false);
            List<ModuleViewModels> viewModels = new List<ModuleViewModels>();
            foreach (var t in modules)
            {
                viewModels.Add(IMapper.Map<ModuleViewModels>(t));
            }
            return GetMenuTrees(viewModels);
        }
        /// <summary>
        /// ��ȡ�˵���
        /// </summary>
        /// <returns></returns>
        public List<ModuleViewModels> GetMenuTrees(List<ModuleViewModels> moduleViewModels)
        {
            //���������˵�
            List<ModuleViewModels> modulePartentModels = new List<ModuleViewModels>();

            //ѭ�������������˵�
            foreach (var t in moduleViewModels)
            {
                if (t.ParentId == 0)
                {
                    modulePartentModels.Add(t);
                    t.Meta.Icon = t.Icon;
                    t.Meta.Title = t.Title;
                    t.Meta.Role = UserRoles;
                }
            }
            //������������˵�
            moduleViewModels.RemoveAll(s => s.ParentId == 0);
            modulePartentModels = BubbleSort(modulePartentModels);
            //һ���˵��Ķ����˵�
            foreach (var t in modulePartentModels)
            {
                t.Children = GetChildrenViewModels(t.Id, moduleViewModels);
            }
            return modulePartentModels;
        }

        /// <summary>
        /// �ݹ��ȡ��ǰ�ĸ������Ӳ˵�
        /// </summary>
        /// <param name="moduleViewModels"></param>
        /// <returns></returns>
        public List<ModuleViewModels> GetChildrenViewModels(int Id, List<ModuleViewModels> moduleViewModels)
        {

            List<ModuleViewModels> childrenViewModels = new List<ModuleViewModels>();
            foreach (var s in moduleViewModels)
            {
                if (s.ParentId == Id)
                {
                    childrenViewModels.Add(s);
                    s.Meta.Icon = s.Icon;
                    s.Meta.Title = s.Title;
                    s.Meta.Role = UserRoles;
                }
            }
            foreach(var a in childrenViewModels)
            {
                moduleViewModels.Remove(a);
            }
            foreach (var t in childrenViewModels)
            {
                //�ݹ�
                t.Children= GetChildrenViewModels(t.Id, moduleViewModels);
            }
            if (childrenViewModels.Count == 0)
            {
                return null;
            }

            return BubbleSort(childrenViewModels);
        }


        /// <summary>
        /// ð������
        /// </summary>
        /// <param name="weighFields"></param>
        /// <returns></returns>
        public List<ModuleViewModels> BubbleSort(List<ModuleViewModels> moduleViewModels)
        {
            var len = moduleViewModels.Count;
            for (var i = 0; i < len - 1; i++)
            {
                for (var j = 0; j < len - 1 - i; j++)
                {

                    if (moduleViewModels[j].OrderSort > moduleViewModels[j + 1].OrderSort)
                    {        // ����Ԫ�������Ա�
                        var temp = moduleViewModels[j + 1];
                        // Ԫ�ؽ���
                        moduleViewModels[j + 1] = moduleViewModels[j];
                        moduleViewModels[j] = temp;
                    }

                }
            }
            return moduleViewModels;
        }

    }
}
