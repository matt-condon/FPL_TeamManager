using FplClient.Data;
using FplManager.Application.Services;
using FplManager.Infrastructure.Constants;
using FplManager.Infrastructure.Models;
using NUnit.Framework;
using System.Collections.Generic;

namespace FPLManager.Unit.Tests.Application.Services
{
    public class SquadRuleServiceTests
    {
        private SquadRuleService _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new SquadRuleService();
        }
        
        [Test]
        public void IsSquadValid_ShouldReturnFalse_WhereNumberOfPlayersFromTeamIsGreaterThanMaxPlayersPerTeam()
        {
            // Arrange
            var mockFplPlayer = new FplPlayer();
            mockFplPlayer.NowCost = 250;
            mockFplPlayer.TeamId = 10;
            var squad = new Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> {
                {
                    FplPlayerPosition.Forward,
                    new List<EvaluatedFplPlayer>
                    {
                        new EvaluatedFplPlayer(mockFplPlayer, 0.01),
                        new EvaluatedFplPlayer(mockFplPlayer, 0.01),
                        new EvaluatedFplPlayer(mockFplPlayer, 0.01),
                        new EvaluatedFplPlayer(mockFplPlayer, 0.01)
                    }
                }
            };

            // Act
            var result = _sut.IsValidSquad(squad);
            
            // Assert
            Assert.False(result);
        }

        [TestCase(SquadRuleConstants.MaxTotalCost + 1)]
        [TestCase(SquadRuleConstants.MinTotalCost - 1)]
        public void IsSquadValid_ShouldReturnFalse_WhereTotalCostOfPlayersIsNotWithinRange(int totalSquadCost)
        {
            // Arrange
            var mockFplPlayer = new FplPlayer();
            mockFplPlayer.NowCost = totalSquadCost;
            mockFplPlayer.TeamId = 10;
            var squad = new Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> {
                {
                    FplPlayerPosition.Forward,
                    new List<EvaluatedFplPlayer>
                    {
                        new EvaluatedFplPlayer(mockFplPlayer, 0.01)
                    }
                }
            };

            // Act
            var result = _sut.IsValidSquad(squad);

            // Assert
            Assert.False(result);
        }

        [Test]
        public void IsSquadValid_ShouldReturnTrue_WhereNumberOfPlayersFromTeamIsLessThanThreeAndCostIsWithinRange()
        {
            // Arrange
            var mockFplPlayer = new FplPlayer();
            mockFplPlayer.NowCost = 330;
            mockFplPlayer.TeamId = 10;
            var squad = new Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> {
                {
                    FplPlayerPosition.Forward,
                    new List<EvaluatedFplPlayer>
                    {
                        new EvaluatedFplPlayer(mockFplPlayer, 0.01),
                        new EvaluatedFplPlayer(mockFplPlayer, 0.01),
                        new EvaluatedFplPlayer(mockFplPlayer, 0.01)
                    }
                }
            };

            // Act
            var result = _sut.IsValidSquad(squad);
            
            // Assert
            Assert.True(result);
        }
    }
}
