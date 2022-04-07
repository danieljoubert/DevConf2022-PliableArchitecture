using Microsoft.Practices.Unity.Configuration;

namespace Demo
{
	public class WorkerThreadExtension : SectionExtension
  {
    public override void AddExtensions(SectionExtensionContext context)
    {
      context.AddElement<WorkerThreadElement>("workerThread");
    }
  }
}

