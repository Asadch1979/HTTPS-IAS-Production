    (function () {
        // Ensure base url exists (layout/site.js usually defines it)
        var baseUrl = window.g_asiBaseURL || (document.querySelector('meta[name="base-url"]') && document.querySelector('meta[name="base-url"]').content) || "";
        window.g_asiBaseURL = baseUrl;

        var g_engId = 0;
        var g_planId = 0;
        var g_entityId = 0;
        var g_auditById = 0;

        function toDateInputValue(d) {
            if (!d) return "";
            d = ("" + d).trim();

            // Already yyyy-mm-dd (HTML date input format)
            if (/^\d{4}-\d{2}-\d{2}$/.test(d)) return d;

            // dd/mm/yyyy or dd-mm-yyyy (with or without time portion)
            var m = d.match(/^(\d{1,2})[\/-](\d{1,2})[\/-](\d{4})/);
            if (!m) return "";

            var dd = m[1].padStart(2, "0");
            var mm = m[2].padStart(2, "0");
            var yyyy = m[3];
            return yyyy + "-" + mm + "-" + dd;
        }

        function showCommentsModal() {
            var el = document.getElementById("commentsBox");
            if (window.bootstrap && window.bootstrap.Modal && el) {
                window.bootstrap.Modal.getOrCreateInstance(el).show();
                return;
            }
            // Bootstrap 4 fallback
            if (window.jQuery && jQuery.fn && jQuery.fn.modal) {
                $("#commentsBox").modal("show");
            }
        }

        function reloadLocation() {
            if (window.planningDashboard && typeof window.planningDashboard.reloadCurrentStep === 'function') {
                window.planningDashboard.reloadCurrentStep();
                return;
            }

            location.reload();
        }

        function loadTeams(deptCode, selectedTeamId) {
            var $team = $("#teamSelectionBox");
            $team.empty().append('<option value="0">Loading...</option>');

            return $.ajax({
                url: baseUrl + "/ApiCalls/GetAuditTeams",
                type: "POST",
                data: { dept_code: deptCode }, // matches original endpoint usage
                cache: false,
                dataType: "json"
            }).done(function (data) {
                $team.empty().append('<option value="0">--Select Audit Team--</option>');

                if (Array.isArray(data)) {
                    data.forEach(function (team) {
                        var isLead = (team.iS_TEAMLEAD ?? team.IS_TEAMLEAD ?? team.is_TEAMLEAD ?? team.is_teamlead ?? team.isTeamLead);
                        var code = (team.code ?? team.CODE ?? team.team_id ?? team.TEAM_ID);
                        var name = (team.name ?? team.NAME ?? team.team_name ?? team.TEAM_NAME);

                        if (String(isLead).toUpperCase() === "Y") {
                            $team.append('<option value="' + code + '">' + name + '</option>');
                        }
                    });
                }

                if (selectedTeamId !== undefined && selectedTeamId !== null) {
                    $team.val(String(selectedTeamId));
                }
            }).fail(function (xhr) {
                console.log("Error loading teams:", xhr && (xhr.responseText || xhr));
                $team.empty().append('<option value="0">--Select Audit Team--</option>');
                alert("Unable to load audit teams.");
            });
        }

        // Global so handlers can call it
        window.reRecommendEngagementPlan = function (
            engId, planId, entityId,
            startDate, endDate,
            op_startDate, op_endDate,
            teamId, auditById,
            comments
        ) {
            g_engId = Number(engId) || 0;
            g_planId = Number(planId) || 0;
            g_entityId = Number(entityId) || 0;
            g_auditById = Number(auditById) || 0;

            $("#commentsAreaEnteredBox").val(comments || "");

            $("#selectOPDateInputField").val(toDateInputValue(op_startDate));
            $("#selectOPEndDateInputField").val(toDateInputValue(op_endDate));
            $("#selectDateInputField").val(toDateInputValue(startDate));
            $("#selectEndDateInputField").val(toDateInputValue(endDate));

            showCommentsModal();

            if (g_auditById) {
                loadTeams(g_auditById, teamId);
            } else {
                $("#teamSelectionBox").empty().append('<option value="0">--Select Audit Team--</option>').val(String(teamId || "0"));
            }
        };

        window.finalreRecommendEngagementPlan = function () {
            var comments = $("#commentsAreaEnteredBox").val();
            if (!comments || !comments.trim()) { alert("Enter Comments to Proceed"); return false; }

            var ops = $("#selectOPDateInputField").val();
            var ope = $("#selectOPEndDateInputField").val();
            var s = $("#selectDateInputField").val();
            var se = $("#selectEndDateInputField").val();

            if (!ops || !ope || !s || !se) { alert("Please select all dates to proceed."); return false; }

            var teamId = $("#teamSelectionBox").val();
            if (!teamId || teamId === "0") { alert("Select Team to Proceed"); return false; }

            $.ajax({
                url: baseUrl + "/ApiCalls/rerecommend_engagement_plan",
                type: "POST",
                data: {
                    ENG_ID: g_engId,
                    PLAN_ID: g_planId,
                    ENTITY_ID: g_entityId,
                    COMMENTS: comments,
                    OP_START_DATE: ops,
                    OP_END_DATE: ope,
                    START_DATE: s,
                    END_DATE: se,
                    TEAM_ID: teamId
                },
                cache: false,
                dataType: "json"
            }).done(function (data) {
                showApiAlert(data, "Saved successfully.");
                if (typeof onAlertCallback === "function") {
                    onAlertCallback(reloadLocation);
                } else {
                    reloadLocation();
                }
            }).fail(function (xhr) {
                console.log("Submit error:", xhr && (xhr.responseText || xhr));
                showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), "Unable to submit. Please try again.");
            });
        };

        window.approveEngagementPlan = function (engId) {
            $.ajax({
                url: baseUrl + "/ApiCalls/approve_engagement_plan",
                type: "POST",
                data: { ENG_ID: engId },
                cache: false,
                dataType: "json"
            }).done(function () {
                alert("Successfully approved Engagement Plan");
                if (typeof onAlertCallback === "function") {
                    onAlertCallback(reloadLocation);
                } else {
                    reloadLocation();
                }
            }).fail(function (xhr) {
                console.log("Approve error:", xhr && (xhr.responseText || xhr));
                alert("Unable to approve. Please try again.");
            });
        };

        // Delegated click handler (no inline JS needed)
        $(document).on("click", "a.js-recommend-again", function (e) {
            e.preventDefault();
            var d = this.dataset;

            window.reRecommendEngagementPlan(
                d.engId, d.planId, d.entityId,
                d.startDate, d.endDate,
                d.opStartDate, d.opEndDate,
                d.teamId, d.auditById,
                d.comments
            );
        });
    })();
