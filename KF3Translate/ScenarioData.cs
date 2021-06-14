using System.Collections.Generic;

namespace KF3Translate
{
    public class Scenario
    {
        public class CharaData
        {
            public string model { get; set; }
            public string name { get; set; }
        }

        public class RowData
        {
            public int mType { get; set; }
            public int mSerifCharaID { get; set; }
            public string mSerifCharaName { get; set; }
            public int arrayNum { get; set; }
            public List<int> ID { get; set; }
            public List<int> mCharaPosition { get; set; }
            public List<int> mCharaMove { get; set; }
            public List<int> mModelMotion { get; set; }
            public List<int> mMotionFade { get; set; }
            public List<string> mModelFaceId { get; set; }
            public List<string> mIdleFaceId { get; set; }
            public List<int> mCharaEffect { get; set; }
            public List<int> mCharaFaceRot { get; set; }
            public List<string> mStrParams { get; set; }
            public List<int> mIntParams { get; set; }
            public List<double> mFloatParams { get; set; }
        }

        public int m_Enabled { get; set; }
        public string m_Name { get; set; }
        public string mTitleName { get; set; }
        public int ImpossibleSkip { get; set; }
        public List<object> cueSheetList { get; set; }
        public List<object> seSheetList { get; set; }
        public List<string> effectSheetList { get; set; }
        public List<CharaData> charaDatas { get; set; }
        public List<object> miraiDatas { get; set; }
        public List<RowData> rowDatas { get; set; }
    }
}
