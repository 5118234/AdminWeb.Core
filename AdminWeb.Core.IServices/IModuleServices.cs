using AdminWeb.Core.IServices.BASE;
using AdminWeb.Core.Model;
using AdminWeb.Core.Model.Models;
using AdminWeb.Core.Model.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdminWeb.Core.IServices
{	
	/// <summary>
	/// ModuleServices
	/// </summary>	
    public interface IModuleServices :IBaseServices<Module>
	{
        /// <summary>
        /// ��ȡ�����˵�
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        Task<ModuleViewModels> GetModule(int Id);
        /// <summary>
        /// ��ȡ�˵���ҳ
        /// </summary>
        /// <param name="moduleViewModels"></param>
        /// <returns></returns>
        TableModel<ModuleViewModels> ListPageModules(ModuleViewModels moduleViewModels);
        /// <summary>
        /// ��ȡ��ǰ���õĲ˵�·��
        /// </summary>
        /// <returns></returns>
        List<ModuleViewModels> ListModules();
        /// <summary>
        /// ����µĲ˵�
        /// </summary>
        /// <param name="moduleViewModels"></param>
        /// <returns></returns>

        Task<bool> AddModule(ModuleViewModels moduleViewModels);
        /// <summary>
        /// ���µ�ǰ�˵�
        /// </summary>
        /// <param name="moduleViewModels"></param>
        /// <returns></returns>
        Task<bool> UpdateModule(ModuleViewModels moduleViewModels);
        /// <summary>
        /// ���ϲ˵�
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        Task<bool> DeleteModule(int Id);

    }
}
