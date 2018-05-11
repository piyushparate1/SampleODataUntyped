using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.OData;
using System.Web.OData.Extensions;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using Microsoft.OData.Edm;
using System;

namespace ODataUntypedSample.Controllers
{
	public class CustomersController : ODataController
	{
		public EdmEntityObjectCollection Get()
		{
			IEdmEntityType customerType = (IEdmEntityType)ODataUntypedSample.Model.FindType("NS.Customer");
			IEdmCollectionType collectionType = Request.ODataProperties().Path.EdmType as IEdmCollectionType;
			EdmEntityObjectCollection collection = new EdmEntityObjectCollection(new EdmCollectionTypeReference(collectionType));

			EdmEntityObject customer1 = new EdmEntityObject(customerType);
			customer1.TrySetPropertyValue("CustomerId", CUSTOMER1id);
			customer1.TrySetPropertyValue("FullName", "Jony Mark One");
			collection.Add(customer1);

			EdmEntityObject customer2 = new EdmEntityObject(customerType);
			customer2.TrySetPropertyValue("CustomerId", CUSTOMER2id);
			customer2.TrySetPropertyValue("FullName", "Pole Sam two");
			customer2.TrySetPropertyValue("Products", GetChildProduct());
			collection.Add(customer2);
			
			return collection;
		}

		private static EdmEntityObjectCollection GetChildProduct()
		{
			IEdmEntityType productType = (IEdmEntityType)ODataUntypedSample.Model.FindType("NS.Product");

			IEdmCollectionTypeReference entityCollectionType
							= new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(productType, isNullable: false)));
			EdmEntityObjectCollection ec = new EdmEntityObjectCollection(entityCollectionType);

			EdmEntityObject product1 = new EdmEntityObject(productType);
			product1.TrySetPropertyValue("ProductId", PRODUCT1id);
			product1.TrySetPropertyValue("CustomerIdRef", CUSTOMER2id);
			ec.Add(product1);

			EdmEntityObject product2 = new EdmEntityObject(productType);
			product2.TrySetPropertyValue("ProductId", PRODUCT2id);
			product2.TrySetPropertyValue("CustomerIdRef", CUSTOMER2id);
			ec.Add(product2);

			return ec;
		}

		private static Guid CUSTOMER1id = Guid.NewGuid();
		private static Guid CUSTOMER2id = Guid.NewGuid();
		private static Guid PRODUCT1id = Guid.NewGuid();
		private static Guid PRODUCT2id = Guid.NewGuid();
	}
}
