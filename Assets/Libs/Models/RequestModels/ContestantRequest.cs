namespace Libs.Models.RequestModels
{
    [System.Serializable]
    public class ContestantRequest
    {
        public string Id;
        public string Name;
        public double Coefficient;
        public bool Winner;
    }
}