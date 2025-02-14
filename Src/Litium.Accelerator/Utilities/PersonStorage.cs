using System;
using System.Collections.Generic;
using System.Linq;
using Litium.Customers;
using Litium.Accelerator.Constants;
using Litium.Security;
using Litium.Runtime.DependencyInjection;
using System.Security.Claims;

namespace Litium.Accelerator.Utilities
{
    /// <summary>
    /// Storage utility class for storing person information in session.
    /// </summary>
    [Service(ServiceType = typeof(PersonStorage), Lifetime = DependencyLifetime.Singleton)]
    public class PersonStorage
    {
        private readonly OrganizationService _organizationService;
        private readonly PersonService _personService;
        private readonly RoleService _roleService;
        private readonly SecurityContextService _securityContextService;
        private readonly AuthenticationService _authenticationService;
        private readonly PrincipalContextService _principalContextService;

        public PersonStorage(
            OrganizationService organizationService,
            PersonService personService,
            RoleService roleService,
            SecurityContextService securityContextService,
            AuthenticationService authenticationService,
            PrincipalContextService principalContextService)
        {
            _organizationService = organizationService;
            _personService = personService;
            _roleService = roleService;
            _securityContextService = securityContextService;
            _authenticationService = authenticationService;
            _principalContextService = principalContextService;
        }

        /// <summary>
        /// Gets or sets the <see cref="Organization.SystemId" /> stored in session.
        /// </summary>
        public Guid? CurrentSelectedOrganizationSystemId
        {
            get => _securityContextService.GetIdentitySelectedOrganizationSystemId();
            set
            {
                var personSystemId = _securityContextService.GetIdentityUserSystemId();
                if (personSystemId is null)
                {
                    return;
                }

                var person = _personService.Get(personSystemId.Value);
                var roles = new List<Role>();

                if (value is not null)
                {
                    var organizationLink = person.OrganizationLinks.FirstOrDefault(x => x.OrganizationSystemId == value);
                    if (organizationLink is not null)
                    {
                        roles = organizationLink.RoleSystemIds.Select(x => _roleService.Get(x)).ToList();
                    }
                }

                var hasplacerRole = roles.Exists(item => item.Id == RolesConstants.RoleOrderPlacer);
                var hasApproverRole = roles.Exists(item => item.Id == RolesConstants.RoleOrderApprover);

                var identity = _securityContextService.CreateClaimsIdentity(person.LoginCredential.Username, person.SystemId);
                if (value is null)
                {
                    identity = identity.WithoutSelectedOrganization();
                }
                else
                {
                    identity = identity.WithSelectedOrganization(value.Value);
                }

                identity.AddClaim(new Claim(AcceleratorClaimTypes.HasUserApproverRole, hasApproverRole.ToString()));
                identity.AddClaim(new Claim(AcceleratorClaimTypes.HasUserPlacerRole, hasplacerRole.ToString()));
                _securityContextService.ActAs(identity);
                _authenticationService.RefreshSignIn();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Organization" /> stored in session. This <see cref="Organization" /> contains
        /// information about selected organization for the logged in customer.
        /// </summary>
        public Organization CurrentSelectedOrganization
        {
            get
            {
                var organizationSystemId = CurrentSelectedOrganizationSystemId;
                if (organizationSystemId is not null)
                {
                    return _organizationService.Get(organizationSystemId.Value);
                }

                return null;
            }
            set
            {
                CurrentSelectedOrganizationSystemId = value?.SystemId;
            }
        }

        /// <summary>
        ///     Define if user has approver role
        /// </summary>
        public bool HasApproverRole => _principalContextService.GetCurrentPrincipal()?.FindFirstValue(AcceleratorClaimTypes.HasUserApproverRole) == bool.TrueString;

        /// <summary>
        ///     Define if user has placer role
        /// </summary>
        public bool HasPlacerRole => _principalContextService.GetCurrentPrincipal()?.FindFirstValue(AcceleratorClaimTypes.HasUserPlacerRole) == bool.TrueString;
    }
}
