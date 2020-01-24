﻿using System;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace Volo.Abp.FeatureManagement
{
    public class FeatureManager_Tests : FeatureManagementDomainTestBase
    {
        private readonly IFeatureManager _featureManager;
        private readonly ICurrentTenant _currentTenant;
        private readonly IFeatureChecker _featureChecker;

        public FeatureManager_Tests()
        {
            _featureManager = GetRequiredService<IFeatureManager>();
            _featureChecker = GetRequiredService<IFeatureChecker>();
            _currentTenant = GetRequiredService<ICurrentTenant>();
        }

        [Fact]
        public async Task Should_Get_A_FeatureValue_For_A_Provider()
        {
            //Default values

            (await _featureManager.GetOrNullDefaultAsync(
                TestFeatureDefinitionProvider.SocialLogins
            ).ConfigureAwait(false)).ShouldBeNull();

            (await _featureManager.GetOrNullDefaultAsync(
                TestFeatureDefinitionProvider.DailyAnalysis
            ).ConfigureAwait(false)).ShouldBe(false.ToString().ToLowerInvariant());

            (await _featureManager.GetOrNullDefaultAsync(
                TestFeatureDefinitionProvider.ProjectCount
            ).ConfigureAwait(false)).ShouldBe("1");

            (await _featureManager.GetOrNullDefaultAsync(
                TestFeatureDefinitionProvider.BackupCount
            ).ConfigureAwait(false)).ShouldBe("0");

            //"Enterprise" edition values

            (await _featureManager.GetOrNullForEditionAsync(
                TestFeatureDefinitionProvider.SocialLogins,
                TestEditionIds.Enterprise
            ).ConfigureAwait(false)).ShouldBe(true.ToString().ToLowerInvariant());

            (await _featureManager.GetOrNullForEditionAsync(
                TestFeatureDefinitionProvider.DailyAnalysis,
                TestEditionIds.Enterprise
            ).ConfigureAwait(false)).ShouldBe(false.ToString().ToLowerInvariant());

            (await _featureManager.GetOrNullForEditionAsync(
                TestFeatureDefinitionProvider.ProjectCount,
                TestEditionIds.Enterprise
            ).ConfigureAwait(false)).ShouldBe("3");

            (await _featureManager.GetOrNullForEditionAsync(
                TestFeatureDefinitionProvider.BackupCount,
                TestEditionIds.Enterprise
            ).ConfigureAwait(false)).ShouldBe("5");

            //"Ultimate" edition values

            (await _featureManager.GetOrNullForEditionAsync(
                TestFeatureDefinitionProvider.SocialLogins,
                TestEditionIds.Ultimate
            ).ConfigureAwait(false)).ShouldBe(true.ToString().ToLowerInvariant());

            (await _featureManager.GetOrNullForEditionAsync(
                TestFeatureDefinitionProvider.DailyAnalysis,
                TestEditionIds.Ultimate
            ).ConfigureAwait(false)).ShouldBe(true.ToString().ToLowerInvariant());

            (await _featureManager.GetOrNullForEditionAsync(
                TestFeatureDefinitionProvider.ProjectCount,
                TestEditionIds.Ultimate
            ).ConfigureAwait(false)).ShouldBe("10");

            (await _featureManager.GetOrNullForEditionAsync(
                TestFeatureDefinitionProvider.BackupCount,
                TestEditionIds.Ultimate
            ).ConfigureAwait(false)).ShouldBe("10");
        }

        [Fact]
        public async Task Should_Change_Feature_Value_And_Refresh_Cache()
        {
            var tenantId = Guid.NewGuid();

            //It is "False" at the beginning
            using (_currentTenant.Change(tenantId))
            {
                (await _featureChecker.IsEnabledAsync(TestFeatureDefinitionProvider.SocialLogins)).ShouldBeFalse();
            }

            //Set to "True" by host for the tenant
            using (_currentTenant.Change(null))
            {
                (await _featureChecker.IsEnabledAsync(TestFeatureDefinitionProvider.SocialLogins)).ShouldBeFalse();
                await _featureManager.SetForTenantAsync(tenantId, TestFeatureDefinitionProvider.SocialLogins, "True");
                (await _featureManager.GetOrNullForTenantAsync(TestFeatureDefinitionProvider.SocialLogins, tenantId)).ShouldBe("True");
            }

            //Now, it should be "True"
            using (_currentTenant.Change(tenantId))
            {
                (await _featureChecker.IsEnabledAsync(TestFeatureDefinitionProvider.SocialLogins)).ShouldBeTrue();
            }
        }
    }
}
