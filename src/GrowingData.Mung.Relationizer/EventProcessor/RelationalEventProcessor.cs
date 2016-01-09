using GrowingData.Mung.Core;

namespace GrowingData.Mung.Relationizer {
	public class RelationalEventProcessor : EventProcessor {

		private RelationalSchema _relationizer;

		public RelationalEventProcessor(string basePath)
			: base("RelationalEventWriter") {
			_relationizer = new RelationalSchema(basePath);

		}

		protected override void ProcessEvent(MungServerEvent evt) {
			_relationizer.Write(evt);


		}


	}
}
