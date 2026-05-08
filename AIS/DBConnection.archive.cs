using AIS.Models;
using AIS.Models.AIS.Models.Execution;
using AIS.Models.Reports;
using System.Collections.Generic;

namespace AIS.Controllers
    {
    public class DBConnectionArchive
        {
        private readonly DBConnection _dbConnection;

        public DBConnectionArchive(DBConnection dbConnection)
            {
            _dbConnection = dbConnection;
            }

        public object GetAuditeeAssignedEntities() => _dbConnection.GetAuditeeAssignedEntities();
        public object GetAuditeeOldParasEntities() => _dbConnection.GetAuditeeOldParasEntities();
        public object GetAuditVoilationcats() => _dbConnection.GetAuditVoilationcats();
        public object GetAuditChecklistCAD() => _dbConnection.GetAuditChecklistCAD();
        public object GetRisks() => _dbConnection.GetRisks();
        public object GetFunctionalListForDashboard() => _dbConnection.GetFunctionalListForDashboard();
        public object GetRiskProcessDefinition() => _dbConnection.GetRiskProcessDefinition();
        public object GetAnnexuresForChecklistDetail() => _dbConnection.GetAnnexuresForChecklistDetail();
        public object GetrealtionshiptypeForDashboardPanel() => _dbConnection.GetrealtionshiptypeForDashboardPanel();
        public object GetEntitiesDropDownForManageObservations() => _dbConnection.GetEntitiesDropDownForManageObservations();
        public object GetLegacyParasEntities() => _dbConnection.GetLegacyParasEntities();
        public object GetStaffPosition() => _dbConnection.GetStaffPosition();
        public object GetDepartments(int pageId) => _dbConnection.GetDepartments(pageId);
        public object GetDivisions(bool includeAll) => _dbConnection.GetDivisions(includeAll);
        public object GetZones() => _dbConnection.GetZones();
        public object GetDuplicateParasAuthorizationEntityList() => _dbConnection.GetDuplicateParasAuthorizationEntityList();
        public object GetAuditEntitiesForOtherEntitySelection() => _dbConnection.GetAuditEntitiesForOtherEntitySelection();
        public object GetDepositCat() => _dbConnection.GetDepositCat();
        public object GetLoansScheme(int engId) => _dbConnection.GetLoansScheme(engId);
        public object GetLoansSchemeYearly(int engId) => _dbConnection.GetLoansSchemeYearly(engId);
        public object GetAuditeeEntitiesType() => _dbConnection.GetAuditeeEntitiesType();
        public object GetAuditYearForAddLegacyPara() => _dbConnection.GetAuditYearForAddLegacyPara();
        public object GetAuditNatureForAddLegacyPara() => _dbConnection.GetAuditNatureForAddLegacyPara();
        public object GetLegacyParasEntitiesHO() => _dbConnection.GetLegacyParasEntitiesHO();
        public object GetLegacyParasEntitiesFAD() => _dbConnection.GetLegacyParasEntitiesFAD();
        public object GetGlheadDetails(int engId) => _dbConnection.GetGlheadDetails(engId);
        public object GetControlViolations() => _dbConnection.GetControlViolations();
        public object Getrealtionshiptype(int pageId) => _dbConnection.Getrealtionshiptype(pageId);
        public object GetCurrentParasEntitiesForStatusChange() => _dbConnection.GetCurrentParasEntitiesForStatusChange();

        public List<AuditeeOldParasModel> GetAuditeeOldParas(int entityId = 0) => _dbConnection.GetAuditeeOldParas(entityId);
        public List<OldParasModelCAD> GetOldParasManagement() => _dbConnection.GetOldParasManagement();
        public string AddOldParasCADReply(int id, int violationCategoryId, int violationNatureId, int riskId, string reply) =>
            _dbConnection.AddOldParasCADReply(id, violationCategoryId, violationNatureId, riskId, reply);
        public string AddOldParasCADCompliance(OldParaComplianceModel compliance) => _dbConnection.AddOldParasCADCompliance(compliance);
        public List<GetOldParasBranchComplianceModel> GetOldParasBranchComplianceTextupdate() => _dbConnection.GetOldParasBranchComplianceTextupdate();
        public List<EntityWiseObservationModel> GetReportingOfficeWiseObservations() => _dbConnection.GetReportingOfficeWiseObservations();
        public List<NoEntitiesRiskBasePlan> GetEntitiesRiskBasePlanForDashboard() => _dbConnection.GetEntitiesRiskBasePlanForDashboard();
        public List<FunctionalResponsibilitiesWiseParasModel> GetFunctionalResponsibilityWiseParaForDashboard(int functionalEntityId = 0) =>
            _dbConnection.GetFunctionalResponsibilityWiseParaForDashboard(functionalEntityId);
        public List<FADNewOldParaPerformanceModel> GetHOFunctionalResponsibilityWiseParaForDashboard(int processId = 0, int subProcessId = 0, int processDetailId = 0, int functionalEntityId = 0, int deptId = 0) =>
            _dbConnection.GetHOFunctionalResponsibilityWiseParaForDashboard(processId, subProcessId, processDetailId, functionalEntityId, deptId);
        public List<RiskProcessDefinition> GetHOViolationListForDashboard(int entityId = 0) => _dbConnection.GetHOViolationListForDashboard(entityId);
        public List<RiskProcessDefinition> GetHOSubViolationListForDashboard(int entityId = 0, int processId = 0) => _dbConnection.GetHOSubViolationListForDashboard(entityId, processId);
        public List<FunctionalAnnexureWiseObservationModel> GetFunctionalObservations(int annexId, int entityId) => _dbConnection.GetFunctionalObservations(annexId, entityId);
        public List<AuditEntitiesModel> GetAuditEntitiesByTypeId(int entityTypeId) => _dbConnection.GetAuditEntitiesByTypeId(entityTypeId);
        public string AddNewLegacyPara(AddNewLegacyParaModel legacyPara) => _dbConnection.AddNewLegacyPara(legacyPara);
        public List<AddNewLegacyParaModel> GetAddedLegacyParaForAuthorize() => _dbConnection.GetAddedLegacyParaForAuthorize();
        public string AuthorizeLegacyParaAddition(string paraRef) => _dbConnection.AuthorizeLegacyParaAddition(paraRef);
        public string DeleteLegacyParaAdditionRequest(string paraRef) => _dbConnection.DeleteLegacyParaAdditionRequest(paraRef);
        public List<OldParasAuthorizeModel> GetOldSettledParasForResponseAuthorize() => _dbConnection.GetOldSettledParasForResponseAuthorize();
        public string AddAuthorizeChangeStatusRequestForSettledPara(string refId, string obsId, string indicator, string actionIndicator) =>
            _dbConnection.AddAuthorizeChangeStatusRequestForSettledPara(refId, obsId, indicator, actionIndicator);
        public List<AddNewLegacyParaModel> GetUpdatedGistParaOfLegacyParaForAuthorize() => _dbConnection.GetUpdatedGistParaOfLegacyParaForAuthorize();
        public string AuthorizeLegacyParaGistParaNoUpdate(string paraRef, string gistOfPara, string paraNo) =>
            _dbConnection.AuthorizeLegacyParaGistParaNoUpdate(paraRef, gistOfPara, paraNo);
        public List<DuplicateDeleteManageParaModel> GetDuplicateParasForAuthorization() => _dbConnection.GetDuplicateParasForAuthorization();
        public string RejectDeleteDuplicatePara(int duplicateParaId = 0) => _dbConnection.RejectDeleteDuplicatePara(duplicateParaId);
        public string AuthDeleteDuplicatePara(int duplicateParaId = 0) => _dbConnection.AuthDeleteDuplicatePara(duplicateParaId);
        public List<DepositAccountModel> GetDepositAccountSubdetails(string branchName) => _dbConnection.GetDepositAccountSubdetails(branchName);
        public List<DepositAccountCatDetailsModel> GetDepositAccountcatdetails(int categoryId) => _dbConnection.GetDepositAccountcatdetails(categoryId);
        public List<GlHeadDetailsModel> GetIncomeExpenceDetails(int branchId, int engId) => _dbConnection.GetIncomeExpenceDetails(branchId, engId);
        public List<OldParasModel> GetCurrentParasForStatusChangeRequestAuthorize() => _dbConnection.GetCurrentParasForStatusChangeRequestAuthorize();
        public string AddChangeStatusRequestForCurrentPara(string refId, int newStatus, string remarks) => _dbConnection.AddChangeStatusRequestForCurrentPara(refId, newStatus, remarks);
        public List<ParaStatusChangeModel> GetParasForStatusChange(int entityId = 0) => _dbConnection.GetParasForStatusChange(entityId);
        public string AddChangeStatusRequestForPara(string comId, int newStatus, string remarks, string indicator, string actionIndicator) =>
            _dbConnection.AddChangeStatusRequestForPara(comId, newStatus, remarks, indicator, actionIndicator);
        public List<ParaStatusChangeModel> GetParasForStatusChangeToAuthorize() => _dbConnection.GetParasForStatusChangeToAuthorize();
        public string AuthorizeChangeStatusRequestForPara(string comId, int newParaId, int oldParaId, string remarks, string indicator, string actionIndicator) =>
            _dbConnection.AuthorizeChangeStatusRequestForPara(comId, newParaId, oldParaId, remarks, indicator, actionIndicator);
        }
    }
