using System.Collections.Generic;
using System.Linq;
using GitSearch2.Shared;
using Microsoft.AspNetCore.Components;

namespace GitSearch2.Client.Shared.Components {
	public partial class CommitDetailsView {

		[Parameter] public CommitDetails Details { get; set; }

		[Inject] protected IRepoWebsiteIdentifier RepoIdentifier { get; set; }

		public string CommitUrl { get; set; }

		public string PrUrl { get; set; }

		public IDictionary<string, string> MergeUrls { get; set; }

		public IDictionary<string, string> FileUrls { get; set; }

		protected override void OnParametersSet() {
			string repo = Details.Repo;
			string project = Details.Project;

			(string Project, string Repo, string Sha) = HandleSubRepo( Details.Description, project, repo, Details.CommitId );
			repo = Repo;
			project = Project;

			IUrlGenerator urlGenerator = RepoIdentifier.GetUrlGenerator( Details.OriginId );
			CommitUrl = urlGenerator.CommitUrl( Details );
			PrUrl = urlGenerator.PrUrl( project, repo, Details );

			MergeUrls = Details.Commits.ToDictionary( c => c, c => urlGenerator.MergeUrl( Details, c ) );

			FileUrls = Details.Files.ToDictionary( f => f, f => urlGenerator.FileUrl( Details, f ) );
		}

		private static (string Project, string Repo, string Sha) HandleSubRepo(
			IEnumerable<string> description,
			string project,
			string repo,
			string sha
		) {
			string line = description.FirstOrDefault( d => d.StartsWith( "subrepo:" ) );

			if( line == default ) {
				return (project, repo, sha);
			}

			string subRepo = line["subrepo:".Length..].Trim();
			line = description.FirstOrDefault( d => d.StartsWith( "subrepo-id:" ) );
			string subRepoId = line["subrepo-id:".Length..].Trim();
			(string Project, string Repo) = ConvertSubRepo( subRepo, project, repo );
			return (Project, Repo, subRepoId);
		}

		private static (string Project, string Repo) ConvertSubRepo( string subrepo, string project, string repo ) {
			return ( subrepo.ToLower() ) switch {
				"adaptive-learning-lms" => ("aln", "adaptive-learning-lms"),
				"ae" => ("an", "ae"),
				"ap" => ("an", "ap"),
				"ap_le" => ("an", "ap_le"),
				"ap_lp" => ("an", "ap_lp"),
				"aw" => ("an", "aw"),
				"dataexportframework" => ("an", "dataexportframework"),
				"datahub" => ("an", "datahub"),
				"s3" => ("an", "s3"),
				"s3.pt" => ("an", "s3_pt"),
				"userinteractionconsumer" => ("an", "userinteractionconsumer"),
				"badging_notifications" => ("bdg", "badging-system-plugin"),
				"aws-ci-workflow" => ("ci", "aws-ci-workflow"),
				"ci" => ("ci", "small-bundle"),
				"campuslife.notifications" => ("cl", "campuslife"),
				"class-stream" => ("clp", "class-stream"),
				"contentservice" => ("contentservice", "contentservice"),
				"apploader" => ("core", "apploader"),
				"binder" => ("core", "binder"),
				"d2l.lp.languageService" => ("core", "d2l.lp.languageservice"),
				"datawarehouse" => ("core", "datawarehouse"),
				"dataWarehouse_platformTools" => ("core", "datawarehouse_platformtools"),
				"gbl" => ("core", "gbl"),
				"guidsso" => ("core", "guidsso"),
				"le" => ("core", "le"),
				"le_platformTools" => ("core", "le_pt"),
				"core" => ("core", "lms"),
				"lp" => ("core", "lp"),
				"manager-role" => ("core", "manager-role"),
				"lp.mediaIntegration" => ("core", "mediaintegration"),
				"orgregressiontests" => ("core", "orgregressiontests"),
				"platformTools" => ("core", "platformtools"),
				"readspeaker.docreader" => ("core", "readspeaker-docreader"),
				"scheduledtasks" => ("core", "scheduledtasks"),
				"selfRegistration" => ("core", "selfregistration"),
				"testing_scaffolds" => ("core", "testing_scaffolds"),
				"testing_scaffolds_plugins" => ("core", "testing_scaffolds_plugins"),
				"lp.webdav" => ("core", "webdav"),
				"ws_le" => ("core", "ws_le"),
				"Custom-EPSharingGroups" => ("cust", "academy-epsharinggroups"),
				"Custom-CourseCompletion" => ("cust", "accenture-coursecompletion"),
				"Custom-Leaderboard" => ("cust", "accenture-leaderboard"),
				"Custom-AdminConsoleWS" => ("cust", "adminconsolews"),
				"Custom-InitiateDocument" => ("cust", "amex-initiatedocument"),
				"Custom-HistoricalDataImport" => ("cust", "ana-historicaldataimport"),
				"Custom-LearnerStatusWidget" => ("cust", "aoe-learnerstatuswidget"),
				"Custom-PasswordExpireWarning" => ("cust", "aoe-passwordexpirewarning"),
				"Custom-ARESExport" => ("cust", "aresexport"),
				"Custom-CertificateTranscripts" => ("cust", "awards-certificatetranscripts"),
				"awards-rfreports" => ("cust", "awards-rfreports"),
				"Custom-RosterReport" => ("cust", "awards-rosterreport"),
				"awards.service" => ("cust", "awards-service"),
				"Custom-SynergeticSSO" => ("cust", "brighton-synergeticsso"),
				"Custom-BulkGradesAPIs" => ("cust", "bulkgradesapis"),
				"Custom-CasUcIdMapper" => ("cust", "casucidmapper"),
				"custom.cas" => ("cust", "common-cas"),
				"common-coursesupdater" => ("cust", "common-coursesupdater"),
				"custom.platform" => ("cust", "common-customizationplatform"),
				"data_export" => ("cust", "common-dataexport"),
				"Custom-EquellaAPIs" => ("cust", "common-equellaapis"),
				"custom-equellahooks" => ("cust", "common-equellahooks"),
				"common-ipsis" => ("cust", "common-ipsis"),
				"Custom-GradesLeaderboard" => ("cust", "common-leaderboard"),
				"common-licensing" => ("cust", "common-licensing"),
				"custom.mapleta" => ("cust", "common-mapleta"),
				"custom.myorgunitswidget" => ("cust", "common-myorgunitswidget"),
				"Custom-MyStudentsWidget" => ("cust", "common-mystudentswidget"),
				"common-oux" => ("cust", "common-oux"),
				"common-pearsonscriptedlinks" => ("cust", "common-pearsonscriptedlinks"),
				"common-updatesproc" => ("cust", "common-updatesproc"),
				"Custom-ContentConversation" => ("cust", "contentconversation"),
				"Custom-ContentLicenseCheck" => ("cust", "contentlicensecheck"),
				"Custom-ContentViewer" => ("cust", "contentviewer"),
				"Custom-DocumentStatus" => ("cust", "cpacanada-documentstatus"),
				"Custom-MailTemplateOrgDefinedId" => ("cust", "cpacanada-mailtemplateorgdefinedidprovider"),
				"Custom-CreateInstructorDevCourse" => ("cust", "createinstructortestcourse"),
				"Custom-CreditUserReset" => ("cust", "credituserreset"),
				"Custom-APIs" => ("cust", "custom-apis"),
				"Custom-Handlers" => ("cust", "custom-handlers"),
				"Custom-Plugins" => ("cust", "custom-plugins"),
				"Custom-ThirdPartyIntegrations" => ("cust", "custom-thirdpartyintegrations"),
				"d2l-visualcourselist" => ("cust", "d2l-visualcourselist"),
				"Custom-DataSets" => ("cust", "datasets"),
				"Custom-DateOffsetAPIs" => ("cust", "dateoffsetapis"),
				"Custom-DBGC" => ("cust", "demographicbasedgroupcreation"),
				"Custom-DEReports" => ("cust", "dereports"),
				"Custom-DropboxScoreRFReport" => ("cust", "dropboxscorerfreport"),
				"Custom-LoginAccessManager" => ("cust", "durham-loginaccessmanager"),
				"Custom-MyCoursesInOtherOrgs" => ("cust", "elo-mycoursesinotherorgs"),
				"Custom-EnrollmentMailTemplates" => ("cust", "enrollmentmailtemplates"),
				"Custom-ePAutoCollections" => ("cust", "epautocollections"),
				"Custom-EReserves" => ("cust", "ereserves"),
				"Custom-ExtendedLTIParameters" => ("cust", "extendedltiparameters"),
				"Custom-BVirtualSSO" => ("cust", "fanshawe-bvirtualsso"),
				"Custom-FailSafeMoveUsers" => ("cust", "fanshawe-failsafemoveusers"),
				"Custom-LegacyInitiateDocument" => ("cust", "gbl-initiatedocument"),
				"Custom-VisionIntegration" => ("cust", "gracehill-visionintegration"),
				"Custom-GradesDisplayWidget" => ("cust", "gradesdisplaywidget"),
				"custom-refererlogout" => ("cust", "guelph-courselinkclo"),
				"Custom-xC0C4DA8D-GradesReport" => ("cust", "guelph-rfreports"),
				"custom-newswidgetpage" => ("cust", "gwinnett-newswidgetpage"),
				"custom-bannerauthplugin" => ("cust", "hacc-bannerauthplugin"),
				"Custom-BannerLogin" => ("cust", "hacc-bannerlogin"),
				"Custom-LTICMSIntegration" => ("cust", "hbp-lticmsintegration"),
				"Custom-OnlyOneActiveByCourseDate" => ("cust", "hbs-onlyoneactivebycoursedateee"),
				"Custom-UserSyncTool" => ("cust", "hrm-usersynctool"),
				"Custom-HyphenboardAPIs" => ("cust", "hyphenboard-valenceapi"),
				"Custom-LDAPMultiServerSupport" => ("cust", "infilaw-ldapmultiserversupport"),
				"internal-jsinjector" => ("cust", "internal-jsinjector"),
				"custom-moocforgotpassword" => ("cust", "internal-moocedudentityforgotpassword"),
				"custom-d2lmoocs" => ("cust", "internal-moocselfregpage"),
				"Custom-IPASDelimitedUserMapping" => ("cust", "ipasdelimitedusermapping"),
				"Custom-NaplanBackend" => ("cust", "isq-naplanbackend"),
				"custom_k12externalhomepageresolver" => ("cust", "k12-externalhomepageresolver"),
				"custom_ipsis" => ("cust", "k12-ipsis"),
				"custom_k12currentcourseselector" => ("cust", "k12-minibar-currentcourseselector"),
				"custom_k12reports" => ("cust", "k12-reportingframeworkreports"),
				"custom_k12tocmanualcompletionremoval" => ("cust", "k12-tocmanualcompletionremoval"),
				"custom_k12widgets" => ("cust", "k12-widgets"),
				"Custom-UserActivityRFReport" => ("cust", "lansing-rfreports"),
				"custom-coursestoragequotas" => ("cust", "lcc-coursestoragequotas"),
				"custom-useractivityreport" => ("cust", "lcc-rfreports"),
				"Custom-ContentDiscussionMappingApi" => ("cust", "lds-contentdiscussionmappingapi"),
				"Custom-DiscussionSummaryWidget" => ("cust", "lds-discussionsummarywidget"),
				"Custom-LocaleSettingsApis" => ("cust", "localesettingsapis"),
				"Custom-LorMetadataImportExport" => ("cust", "lormetadataimportexport"),
				"Custom-OrientationEnrollment" => ("cust", "lscs-orientationenrollment"),
				"Custom-ConnectEDIntegration" => ("cust", "mcgrawhill-connectedintegration"),
				"Custom-MetaViewportLoader" => ("cust", "metaviewportloader"),
				"Custom-MOOCSelfRegistration" => ("cust", "moocselfregistration"),
				"Custom-MyEnrollmentsAPI" => ("cust", "myenrollmentsapis"),
				"custom-dropboxarchiver" => ("cust", "nau-batchdropboxsubmissiondownloads"),
				"Custom-NewestCOUnderOrgByRole" => ("cust", "newestcourseofferingunderorgbyrole"),
				"Custom-CopyCoursesAfterHT" => ("cust", "njvs-copycoursesafterht"),
				"Custom-IPASUserCreateUpdate" => ("cust", "nscc-ipasusercreateupdate"),
				"Custom-OAuth2Authentication" => ("cust", "oauth2authentication"),
				"Custom-FileUsageHarvester" => ("cust", "oup-dereports"),
				"Custom-OAuthSSO" => ("cust", "pearsonbr-oauthsso"),
				"Custom-PersonifySSO" => ("cust", "personifysso"),
				"Custom-ProfileTagLineAPI" => ("cust", "profiletaglineapi"),
				"custom-selfregreport" => ("cust", "pwcs-selfregreport"),
				"Custom-LogoutActionProvider" => ("cust", "queens-logoutactionprovider"),
				"Custom-QuizActionPlan" => ("cust", "quizactionplan"),
				"Custom-RFParticipationReports" => ("cust", "regis-rfreports"),
				"Custom-RestrictedBulkUserManagement" => ("cust", "restrictedbum"),
				"Custom-RF-SISMismatchReport" => ("cust", "rf-sismismatchreport"),
				"Custom-RoleSwitcher" => ("cust", "roleswitcher"),
				"Custom-RubricMapper" => ("cust", "rubricmapper"),
				"Custom-DirectToDepartment" => ("cust", "sanomapro-directtodepartment"),
				"Custom-GradeSchemeWizard" => ("cust", "sanomapro-gradeschemewizard"),
				"Custom-MyAccountSettingsMinibar" => ("cust", "sanomapro-myaccountsettingsminibar"),
				"Custom-ScormReportingTool" => ("cust", "sanomapro-scormreportingtool"),
				"Custom-WorkspaceEnrollment" => ("cust", "sanomapro-workspaceenrollmenttool"),
				"Custom-ScheduledFinalGradeRelease" => ("cust", "scheduledfinalgraderelease"),
				"Custom-ScormDashboard" => ("cust", "scormdashboard"),
				"Custom-AttendanceReport" => ("cust", "sdbor-rfreports"),
				"Custom-SiteCuesIntegration" => ("cust", "sitecuesintegration"),
				"Custom-ProgramManager" => ("cust", "siu-programmanager"),
				"Custom-SpecialAccessApis" => ("cust", "specialaccessapis"),
				"custom-bannerbatchcrosslisting" => ("cust", "standard-bannerbatchcrosslisting"),
				"standard-coursebrandingapi" => ("cust", "standard-coursebrandingapi"),
				"Custom-CourseLastAccessedApi" => ("cust", "standard-courselastaccessedapi"),
				"Custom-StandardCSSInjector" => ("cust", "standard-cssinjector"),
				"Custom-KeyValueStoreApi" => ("cust", "standard-keyvaluestoreapi"),
				"Custom-MultiLdap" => ("cust", "standard-multildap"),
				"custom-studentorientation" => ("cust", "studentorientationsis"),
				"Custom-TalisWidget" => ("cust", "surrey-taliswidget"),
				"custom-termsandconditions" => ("cust", "termsandconditions"),
				"Custom-ThriveIntegration" => ("cust", "thriveintegration"),
				"Custom-SessionCourseCopy" => ("cust", "tui-sessioncoursecopy"),
				"Custom-CloePloeDataSets" => ("cust", "uco-cloeploedatasets"),
				"Custom-CompentencyAPIs" => ("cust", "uco-competenciesapi"),
				"Custom-LearningObjectiveAPI" => ("cust", "uco-learningobjectiveapi"),
				"Custom-UnreadEmailCountAPI" => ("cust", "unreademailcountapi"),
				"Custom-UrkundIntegration" => ("cust", "urkundintegration"),
				"Custom-x820DB485-Reporting" => ("cust", "uwaterloo-customreporting"),
				"Custom-PDGG" => ("cust", "vhsinc-pdgg"),
				"Custom-WorkspacesAPI" => ("cust", "workspacemanagementapis"),
				"ed_lp_consumer" => ("edu", "ed_lp_consumer"),
				"ed_lp_enterprise" => ("edu", "ed_lp_enterprise"),
				"edsdk" => ("edu", "edsdk"),
				"telegraph" => ("ee", "telegraph"),
				"ep" => ("ep", "ep"),
				"ep_le" => ("ep", "ep_le"),
				"ep_pt" => ("ep", "ep_pt"),
				"gs_ed" => ("ep", "gs_ed"),
				"folio" => ("folio", "folio"),
				"im.bannergrades" => ("ie", "banner-grades-export"),
				"im.bic" => ("ie", "bannerrealtime"),
				"im.clmi" => ("ie", "crosslistings"),
				"d2lws" => ("ie", "d2lws"),
				"kaltura" => ("ie", "dms"),
				"d2lws_common" => ("ie", "domain"),
				"ext_ui" => ("ie", "ext_ui"),
				"im.holdingtank" => ("ie", "holding-tank"),
				"im.ht_pt" => ("ie", "holding-tank_pt"),
				"im.holdingtank.system-tests" => ("ie", "holding-tank_tests"),
				"implatform" => ("ie", "implatform"),
				"im.ipas" => ("ie", "ipas"),
				"google.apps" => ("ie", "ipct"),
				"respondus" => ("ie", "ipr"),
				"ipsct" => ("ie", "ipsct"),
				"im.ipsis.platform" => ("ie", "ipsis-platform"),
				"im.ipup" => ("ie", "ipup"),
				"im.spws" => ("ie", "ipup-ws"),
				"im.ipsis.lis" => ("ie", "lis"),
				"office365" => ("ie", "office365"),
				"parentportal" => ("ie", "parentportal"),
				"parentportal-ht" => ("ie", "parentportal-ht"),
				"parentportal-ipsis" => ("ie", "parentportal-ipsis"),
				"im.readspeaker" => ("ie", "readspeaker"),
				"lor" => ("lor", "lor"),
				"lor_le" => ("lor", "lor_le"),
				"lor_pt" => ("lor", "lor_pt"),
				"mediaservice" => ("ms", "mediaservice"),
				"nitroservices" => ("nit", "nitroservices"),
				"tim" => ("ops", "tim"),
				"tim.extensions" => ("ops", "tim-extensions"),
				"bfs" => ("pl", "bfs"),
				"bfs.le" => ("pl", "bfs.le"),
				"d2l" => ("ppt", "d2l"),
				"build" => ("re", "build"),
				"componentBuild" => ("re", "componentbuild"),
				"script-library" => ("shield", "script-library"),
				"uiautomation-tests" => ("ta", "uiautomation-tests"),
				"D2L.DevTools.MigrationTools" => ("tool", "devmigrationtools"),
				"mp" => ("wcs", "mp"),
				"wcs" => ("wcs", "wcs"),
				"wcs_pt" => ("wcs", "wcs_pt"),
				_ => (project, repo),
			};
		}
	}
}
