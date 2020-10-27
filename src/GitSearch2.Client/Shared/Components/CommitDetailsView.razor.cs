using System;
using System.Collections.Generic;
using System.Linq;
using GitSearch2.Client.Service;
using GitSearch2.Shared;
using Microsoft.AspNetCore.Components;

namespace GitSearch2.Client.Shared.Components {
	public class CommitDetailsViewBase: ComponentBase {

		[Parameter] public CommitDetails Details { get; set; }

		[Inject] protected IUrlGenerator UrlGenerator { get; set; }

		public string CommitUrl { get; set; }

		public string PrUrl { get; set; }

		public IDictionary<string, string> MergeUrls { get; set; }

		public IDictionary<string, string> FileUrls { get; set; }

		protected override void OnParametersSet() {
			string repo = Details.Repo;
			string project = Details.Project;

			(string Project, string Repo, string Sha) converted = HandleSubRepo( Details.Description, project, repo, Details.CommitId );
			repo = converted.Repo;
			project = converted.Project;

			CommitUrl = UrlGenerator.CommitUrl( Details );
			PrUrl = UrlGenerator.PrUrl( project, repo, Details );

			MergeUrls = Details.Commits.ToDictionary( c => c, c => UrlGenerator.MergeUrl( Details, c ) );

			FileUrls = Details.Files.ToDictionary( f => f, f => UrlGenerator.FileUrl( Details, f ) );
		}

		private (string Project, string Repo, string Sha) HandleSubRepo(
			IEnumerable<string> description,
			string project,
			string repo,
			string sha
		) {
			string line = description.FirstOrDefault( d => d.StartsWith( "subrepo:" ) );

			if (line == default) {
				return (project, repo, sha);
			}

			string subRepo = line.Substring( "subrepo:".Length ).Trim();
			line = description.FirstOrDefault( d => d.StartsWith( "subrepo-id:" ) );
			string subRepoId = line.Substring( "subrepo-id:".Length ).Trim();
			(string Project, string Repo) converted = ConvertSubRepo( subRepo, project, repo );
			return (converted.Project, converted.Repo, subRepoId);
		}

		private (string Project, string Repo) ConvertSubRepo( string subrepo, string project, string repo ) {
			switch( subrepo.ToLower() ) {
				case "adaptive-learning-lms": return ("aln", "adaptive-learning-lms");
				case "ae": return ("an", "ae");
				case "ap": return ("an", "ap");
				case "ap_le": return ("an", "ap_le");
				case "ap_lp": return ("an", "ap_lp");
				case "aw": return ("an", "aw");
				case "dataexportframework": return ("an", "dataexportframework");
				case "datahub": return ("an", "datahub");
				case "s3": return ("an", "s3");
				case "s3.pt": return ("an", "s3_pt");
				case "userinteractionconsumer": return ("an", "userinteractionconsumer");
				case "badging_notifications": return ("bdg", "badging-system-plugin");
				case "aws-ci-workflow": return ("ci", "aws-ci-workflow");
				case "ci": return ("ci", "small-bundle");
				case "campuslife.notifications": return ("cl", "campuslife");
				case "class-stream": return ("clp", "class-stream");
				case "contentservice": return ("contentservice", "contentservice");
				case "apploader": return ("core", "apploader");
				case "binder": return ("core", "binder");
				case "d2l.lp.languageService": return ("core", "d2l.lp.languageservice");
				case "datawarehouse": return ("core", "datawarehouse");
				case "dataWarehouse_platformTools": return ("core", "datawarehouse_platformtools");
				case "gbl": return ("core", "gbl");
				case "guidsso": return ("core", "guidsso");
				case "le": return ("core", "le");
				case "le_platformTools": return ("core", "le_pt");
				case "core": return ("core", "lms");
				case "lp": return ("core", "lp");
				case "manager-role": return ("core", "manager-role");
				case "lp.mediaIntegration": return ("core", "mediaintegration");
				case "orgregressiontests": return ("core", "orgregressiontests");
				case "platformTools": return ("core", "platformtools");
				case "readspeaker.docreader": return ("core", "readspeaker-docreader");
				case "scheduledtasks": return ("core", "scheduledtasks");
				case "selfRegistration": return ("core", "selfregistration");
				case "testing_scaffolds": return ("core", "testing_scaffolds");
				case "testing_scaffolds_plugins": return ("core", "testing_scaffolds_plugins");
				case "lp.webdav": return ("core", "webdav");
				case "ws_le": return ("core", "ws_le");
				case "Custom-EPSharingGroups": return ("cust", "academy-epsharinggroups");
				case "Custom-CourseCompletion": return ("cust", "accenture-coursecompletion");
				case "Custom-Leaderboard": return ("cust", "accenture-leaderboard");
				case "Custom-AdminConsoleWS": return ("cust", "adminconsolews");
				case "Custom-InitiateDocument": return ("cust", "amex-initiatedocument");
				case "Custom-HistoricalDataImport": return ("cust", "ana-historicaldataimport");
				case "Custom-LearnerStatusWidget": return ("cust", "aoe-learnerstatuswidget");
				case "Custom-PasswordExpireWarning": return ("cust", "aoe-passwordexpirewarning");
				case "Custom-ARESExport": return ("cust", "aresexport");
				case "Custom-CertificateTranscripts": return ("cust", "awards-certificatetranscripts");
				case "awards-rfreports": return ("cust", "awards-rfreports");
				case "Custom-RosterReport": return ("cust", "awards-rosterreport");
				case "awards.service": return ("cust", "awards-service");
				case "Custom-SynergeticSSO": return ("cust", "brighton-synergeticsso");
				case "Custom-BulkGradesAPIs": return ("cust", "bulkgradesapis");
				case "Custom-CasUcIdMapper": return ("cust", "casucidmapper");
				case "custom.cas": return ("cust", "common-cas");
				case "common-coursesupdater": return ("cust", "common-coursesupdater");
				case "custom.platform": return ("cust", "common-customizationplatform");
				case "data_export": return ("cust", "common-dataexport");
				case "Custom-EquellaAPIs": return ("cust", "common-equellaapis");
				case "custom-equellahooks": return ("cust", "common-equellahooks");
				case "common-ipsis": return ("cust", "common-ipsis");
				case "Custom-GradesLeaderboard": return ("cust", "common-leaderboard");
				case "common-licensing": return ("cust", "common-licensing");
				case "custom.mapleta": return ("cust", "common-mapleta");
				case "custom.myorgunitswidget": return ("cust", "common-myorgunitswidget");
				case "Custom-MyStudentsWidget": return ("cust", "common-mystudentswidget");
				case "common-oux": return ("cust", "common-oux");
				case "common-pearsonscriptedlinks": return ("cust", "common-pearsonscriptedlinks");
				case "common-updatesproc": return ("cust", "common-updatesproc");
				case "Custom-ContentConversation": return ("cust", "contentconversation");
				case "Custom-ContentLicenseCheck": return ("cust", "contentlicensecheck");
				case "Custom-ContentViewer": return ("cust", "contentviewer");
				case "Custom-DocumentStatus": return ("cust", "cpacanada-documentstatus");
				case "Custom-MailTemplateOrgDefinedId": return ("cust", "cpacanada-mailtemplateorgdefinedidprovider");
				case "Custom-CreateInstructorDevCourse": return ("cust", "createinstructortestcourse");
				case "Custom-CreditUserReset": return ("cust", "credituserreset");
				case "Custom-APIs": return ("cust", "custom-apis");
				case "Custom-Handlers": return ("cust", "custom-handlers");
				case "Custom-Plugins": return ("cust", "custom-plugins");
				case "Custom-ThirdPartyIntegrations": return ("cust", "custom-thirdpartyintegrations");
				case "d2l-visualcourselist": return ("cust", "d2l-visualcourselist");
				case "Custom-DataSets": return ("cust", "datasets");
				case "Custom-DateOffsetAPIs": return ("cust", "dateoffsetapis");
				case "Custom-DBGC": return ("cust", "demographicbasedgroupcreation");
				case "Custom-DEReports": return ("cust", "dereports");
				case "Custom-DropboxScoreRFReport": return ("cust", "dropboxscorerfreport");
				case "Custom-LoginAccessManager": return ("cust", "durham-loginaccessmanager");
				case "Custom-MyCoursesInOtherOrgs": return ("cust", "elo-mycoursesinotherorgs");
				case "Custom-EnrollmentMailTemplates": return ("cust", "enrollmentmailtemplates");
				case "Custom-ePAutoCollections": return ("cust", "epautocollections");
				case "Custom-EReserves": return ("cust", "ereserves");
				case "Custom-ExtendedLTIParameters": return ("cust", "extendedltiparameters");
				case "Custom-BVirtualSSO": return ("cust", "fanshawe-bvirtualsso");
				case "Custom-FailSafeMoveUsers": return ("cust", "fanshawe-failsafemoveusers");
				case "Custom-LegacyInitiateDocument": return ("cust", "gbl-initiatedocument");
				case "Custom-VisionIntegration": return ("cust", "gracehill-visionintegration");
				case "Custom-GradesDisplayWidget": return ("cust", "gradesdisplaywidget");
				case "custom-refererlogout": return ("cust", "guelph-courselinkclo");
				case "Custom-xC0C4DA8D-GradesReport": return ("cust", "guelph-rfreports");
				case "custom-newswidgetpage": return ("cust", "gwinnett-newswidgetpage");
				case "custom-bannerauthplugin": return ("cust", "hacc-bannerauthplugin");
				case "Custom-BannerLogin": return ("cust", "hacc-bannerlogin");
				case "Custom-LTICMSIntegration": return ("cust", "hbp-lticmsintegration");
				case "Custom-OnlyOneActiveByCourseDate": return ("cust", "hbs-onlyoneactivebycoursedateee");
				case "Custom-UserSyncTool": return ("cust", "hrm-usersynctool");
				case "Custom-HyphenboardAPIs": return ("cust", "hyphenboard-valenceapi");
				case "Custom-LDAPMultiServerSupport": return ("cust", "infilaw-ldapmultiserversupport");
				case "internal-jsinjector": return ("cust", "internal-jsinjector");
				case "custom-moocforgotpassword": return ("cust", "internal-moocedudentityforgotpassword");
				case "custom-d2lmoocs": return ("cust", "internal-moocselfregpage");
				case "Custom-IPASDelimitedUserMapping": return ("cust", "ipasdelimitedusermapping");
				case "Custom-NaplanBackend": return ("cust", "isq-naplanbackend");
				case "custom_k12externalhomepageresolver": return ("cust", "k12-externalhomepageresolver");
				case "custom_ipsis": return ("cust", "k12-ipsis");
				case "custom_k12currentcourseselector": return ("cust", "k12-minibar-currentcourseselector");
				case "custom_k12reports": return ("cust", "k12-reportingframeworkreports");
				case "custom_k12tocmanualcompletionremoval": return ("cust", "k12-tocmanualcompletionremoval");
				case "custom_k12widgets": return ("cust", "k12-widgets");
				case "Custom-UserActivityRFReport": return ("cust", "lansing-rfreports");
				case "custom-coursestoragequotas": return ("cust", "lcc-coursestoragequotas");
				case "custom-useractivityreport": return ("cust", "lcc-rfreports");
				case "Custom-ContentDiscussionMappingApi": return ("cust", "lds-contentdiscussionmappingapi");
				case "Custom-DiscussionSummaryWidget": return ("cust", "lds-discussionsummarywidget");
				case "Custom-LocaleSettingsApis": return ("cust", "localesettingsapis");
				case "Custom-LorMetadataImportExport": return ("cust", "lormetadataimportexport");
				case "Custom-OrientationEnrollment": return ("cust", "lscs-orientationenrollment");
				case "Custom-ConnectEDIntegration": return ("cust", "mcgrawhill-connectedintegration");
				case "Custom-MetaViewportLoader": return ("cust", "metaviewportloader");
				case "Custom-MOOCSelfRegistration": return ("cust", "moocselfregistration");
				case "Custom-MyEnrollmentsAPI": return ("cust", "myenrollmentsapis");
				case "custom-dropboxarchiver": return ("cust", "nau-batchdropboxsubmissiondownloads");
				case "Custom-NewestCOUnderOrgByRole": return ("cust", "newestcourseofferingunderorgbyrole");
				case "Custom-CopyCoursesAfterHT": return ("cust", "njvs-copycoursesafterht");
				case "Custom-IPASUserCreateUpdate": return ("cust", "nscc-ipasusercreateupdate");
				case "Custom-OAuth2Authentication": return ("cust", "oauth2authentication");
				case "Custom-FileUsageHarvester": return ("cust", "oup-dereports");
				case "Custom-OAuthSSO": return ("cust", "pearsonbr-oauthsso");
				case "Custom-PersonifySSO": return ("cust", "personifysso");
				case "Custom-ProfileTagLineAPI": return ("cust", "profiletaglineapi");
				case "custom-selfregreport": return ("cust", "pwcs-selfregreport");
				case "Custom-LogoutActionProvider": return ("cust", "queens-logoutactionprovider");
				case "Custom-QuizActionPlan": return ("cust", "quizactionplan");
				case "Custom-RFParticipationReports": return ("cust", "regis-rfreports");
				case "Custom-RestrictedBulkUserManagement": return ("cust", "restrictedbum");
				case "Custom-RF-SISMismatchReport": return ("cust", "rf-sismismatchreport");
				case "Custom-RoleSwitcher": return ("cust", "roleswitcher");
				case "Custom-RubricMapper": return ("cust", "rubricmapper");
				case "Custom-DirectToDepartment": return ("cust", "sanomapro-directtodepartment");
				case "Custom-GradeSchemeWizard": return ("cust", "sanomapro-gradeschemewizard");
				case "Custom-MyAccountSettingsMinibar": return ("cust", "sanomapro-myaccountsettingsminibar");
				case "Custom-ScormReportingTool": return ("cust", "sanomapro-scormreportingtool");
				case "Custom-WorkspaceEnrollment": return ("cust", "sanomapro-workspaceenrollmenttool");
				case "Custom-ScheduledFinalGradeRelease": return ("cust", "scheduledfinalgraderelease");
				case "Custom-ScormDashboard": return ("cust", "scormdashboard");
				case "Custom-AttendanceReport": return ("cust", "sdbor-rfreports");
				case "Custom-SiteCuesIntegration": return ("cust", "sitecuesintegration");
				case "Custom-ProgramManager": return ("cust", "siu-programmanager");
				case "Custom-SpecialAccessApis": return ("cust", "specialaccessapis");
				case "custom-bannerbatchcrosslisting": return ("cust", "standard-bannerbatchcrosslisting");
				case "standard-coursebrandingapi": return ("cust", "standard-coursebrandingapi");
				case "Custom-CourseLastAccessedApi": return ("cust", "standard-courselastaccessedapi");
				case "Custom-StandardCSSInjector": return ("cust", "standard-cssinjector");
				case "Custom-KeyValueStoreApi": return ("cust", "standard-keyvaluestoreapi");
				case "Custom-MultiLdap": return ("cust", "standard-multildap");
				case "custom-studentorientation": return ("cust", "studentorientationsis");
				case "Custom-TalisWidget": return ("cust", "surrey-taliswidget");
				case "custom-termsandconditions": return ("cust", "termsandconditions");
				case "Custom-ThriveIntegration": return ("cust", "thriveintegration");
				case "Custom-SessionCourseCopy": return ("cust", "tui-sessioncoursecopy");
				case "Custom-CloePloeDataSets": return ("cust", "uco-cloeploedatasets");
				case "Custom-CompentencyAPIs": return ("cust", "uco-competenciesapi");
				case "Custom-LearningObjectiveAPI": return ("cust", "uco-learningobjectiveapi");
				case "Custom-UnreadEmailCountAPI": return ("cust", "unreademailcountapi");
				case "Custom-UrkundIntegration": return ("cust", "urkundintegration");
				case "Custom-x820DB485-Reporting": return ("cust", "uwaterloo-customreporting");
				case "Custom-PDGG": return ("cust", "vhsinc-pdgg");
				case "Custom-WorkspacesAPI": return ("cust", "workspacemanagementapis");
				case "ed_lp_consumer": return ("edu", "ed_lp_consumer");
				case "ed_lp_enterprise": return ("edu", "ed_lp_enterprise");
				case "edsdk": return ("edu", "edsdk");
				case "telegraph": return ("ee", "telegraph");
				case "ep": return ("ep", "ep");
				case "ep_le": return ("ep", "ep_le");
				case "ep_pt": return ("ep", "ep_pt");
				case "gs_ed": return ("ep", "gs_ed");
				case "folio": return ("folio", "folio");
				case "im.bannergrades": return ("ie", "banner-grades-export");
				case "im.bic": return ("ie", "bannerrealtime");
				case "im.clmi": return ("ie", "crosslistings");
				case "d2lws": return ("ie", "d2lws");
				case "kaltura": return ("ie", "dms");
				case "d2lws_common": return ("ie", "domain");
				case "ext_ui": return ("ie", "ext_ui");
				case "im.holdingtank": return ("ie", "holding-tank");
				case "im.ht_pt": return ("ie", "holding-tank_pt");
				case "im.holdingtank.system-tests": return ("ie", "holding-tank_tests");
				case "implatform": return ("ie", "implatform");
				case "im.ipas": return ("ie", "ipas");
				case "google.apps": return ("ie", "ipct");
				case "respondus": return ("ie", "ipr");
				case "ipsct": return ("ie", "ipsct");
				case "im.ipsis.platform": return ("ie", "ipsis-platform");
				case "im.ipup": return ("ie", "ipup");
				case "im.spws": return ("ie", "ipup-ws");
				case "im.ipsis.lis": return ("ie", "lis");
				case "office365": return ("ie", "office365");
				case "parentportal": return ("ie", "parentportal");
				case "parentportal-ht": return ("ie", "parentportal-ht");
				case "parentportal-ipsis": return ("ie", "parentportal-ipsis");
				case "im.readspeaker": return ("ie", "readspeaker");
				case "lor": return ("lor", "lor");
				case "lor_le": return ("lor", "lor_le");
				case "lor_pt": return ("lor", "lor_pt");
				case "mediaservice": return ("ms", "mediaservice");
				case "nitroservices": return ("nit", "nitroservices");
				case "tim": return ("ops", "tim");
				case "tim.extensions": return ("ops", "tim-extensions");
				case "bfs": return ("pl", "bfs");
				case "bfs.le": return ("pl", "bfs.le");
				case "d2l": return ("ppt", "d2l");
				case "build": return ("re", "build");
				case "componentBuild": return ("re", "componentbuild");
				case "script-library": return ("shield", "script-library");
				case "uiautomation-tests": return ("ta", "uiautomation-tests");
				case "D2L.DevTools.MigrationTools": return ("tool", "devmigrationtools");
				case "mp": return ("wcs", "mp");
				case "wcs": return ("wcs", "wcs");
				case "wcs_pt": return ("wcs", "wcs_pt");
				default:
					return (project, repo);
			}
		}
	}
}
