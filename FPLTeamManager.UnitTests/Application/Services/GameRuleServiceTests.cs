using FplClient.Data;
using FPLTeamManager.Application.Services;
using FPLTeamManager.Infrastructure.Constants;
using FPLTeamManager.Infrastructure.Models;
using NUnit.Framework;
using System.Collections.Generic;

namespace FPLTeamManager.UnitTests.Application.Services
{
    public class GameRuleServiceTests
    {
        private GameRuleService _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new GameRuleService();
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
            var result = _sut.IsSquadValid(squad);
            
            // Assert
            Assert.False(result);
        }

        [TestCase(GameRuleConstants.MaxTotalCost + 1)]
        [TestCase(GameRuleConstants.MinTotalCost - 1)]
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
            var result = _sut.IsSquadValid(squad);

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
            var result = _sut.IsSquadValid(squad);
            
            // Assert
            Assert.True(result);
        }
    }
}
