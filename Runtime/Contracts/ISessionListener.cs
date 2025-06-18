using System.Collections.Generic;

namespace AccessVR.OrchestrateVR.SDK
{
    public interface ISessionListener
    {
        public void OnExperiencesChanged(List<LessonData> experiences);
        
        public void OnActiveExperienceChanged(LessonData activeExperience);

        public void OnLogout();

        public void OnUserCode(string userCode);
        
        public void OnUserData(UserData userData);

        public void OnSkybox(string skyboxPath);

        public void OnSubmission(SubmissionData submission);

    }
}