using System;
using System.Collections.Generic;

namespace AIS.Models.Requests
    {
    public class LoginPostModel
        {
        public string PPNumber { get; set; }
        public string Password { get; set; }
        }

    public class ResetPasswordPostModel
        {
        public string PPNumber { get; set; }
        public string CNICNumber { get; set; }
        }

    public class UpdateUserPostModel
        {
        public int? USER_ID { get; set; }
        public int? ROLE_ID { get; set; }
        public int? ENTITY_ID { get; set; }
        public string PASSWORD { get; set; }
        public string EMAIL_ADDRESS { get; set; }
        public string PPNO { get; set; }
        public string ISACTIVE { get; set; }
        }

    public class ContextSelectionPostModel
        {
        public int? AssignmentId { get; set; }
        }

    public class SaveUserContextsPostModel
        {
        public int? USER_ID { get; set; }
        public string PPNO { get; set; }
        public string PASSWORD { get; set; }
        public string EMAIL_ADDRESS { get; set; }
        public string ISACTIVE { get; set; }
        public List<UserContextAssignmentPostModel> ASSIGNMENTS { get; set; } = new List<UserContextAssignmentPostModel>();
        }

    public class UserContextAssignmentPostModel
        {
        public int? ASSIGNMENT_ID { get; set; }
        public int? USER_CONTEXT_ID { get; set; }
        public int? ROLE_ID { get; set; }
        public int? GROUP_ID { get; set; }
        public int? ENTITY_ID { get; set; }
        public int? PARENT_ENTITY_ID { get; set; }
        public int? RELATIONSHIP_TYPE_ID { get; set; }
        public string ISDEFAULT { get; set; }
        public string ISACTIVE { get; set; }
        public string ASSIGNMENT_TYPE { get; set; }
        public string EFFECTIVE_FROM { get; set; }
        public string EFFECTIVE_TO { get; set; }
        public string REMARKS { get; set; }
        public bool ISDELETED { get; set; }
        }

    public class GroupPostModel
        {
        public int? GROUP_ID { get; set; }
        public string GROUP_NAME { get; set; }
        public string GROUP_DESCRIPTION { get; set; }
        public string ISACTIVE { get; set; }
        public int? GROUP_CODE { get; set; }
        }

    public class AddJoiningPostModel
        {
        public int? ID { get; set; }
        public int? ENG_PLAN_ID { get; set; }
        public int? TEAM_MEM_PPNO { get; set; }
        public DateTime? JOINING_DATE { get; set; }
        public DateTime? COMPLETION_DATE { get; set; }
        }

    public class SbpObservationPasswordPostModel
        {
        public string Password { get; set; }
        }
    }
