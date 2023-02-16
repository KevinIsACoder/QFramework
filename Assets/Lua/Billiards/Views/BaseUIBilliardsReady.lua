---@class BaseUIBilliardsReady : UIGameView
local BaseUIBilliardsReady = class("BaseUIBilliardsReady", UIGameView)

-- Properties
BaseUIBilliardsReady.static.prefabPath = "Game/Billiards/Views/UIBilliardsReady.prefab"

function BaseUIBilliardsReady:OnCreated()
	UIGameView.OnCreated(self)
	local coms = self.component:GetComponents(self.transform, true)
	self.view = setmetatable({}, {
		__index = function(t, k)
			local com = nil
			if k == "lblRule" then com = coms:Get(0)
			elseif k == "lblBilliards" then com = coms:Get(1)
			elseif k == "roomerModeLabel" then com = coms:Get(2)
			elseif k == "isMember" then com = coms:Get(3)
			elseif k == "memberText" then com = coms:Get(4)
			elseif k == "maskBg1" then com = coms:Get(5)
			elseif k == "gatherButton" then com = coms:Get(6)
			elseif k == "gatherButtonLabel" then com = coms:Get(7)
			elseif k == "gatherTipButton" then com = coms:Get(8)
			elseif k == "gatherCountdown" then com = coms:Get(9)
			elseif k == "gatherTimeImg" then com = coms:Get(10)
			elseif k == "readyButton" then com = coms:Get(11)
			elseif k == "readyNormal" then com = coms:Get(12)
			elseif k == "readyForbit" then com = coms:Get(13)
			elseif k == "readyTipNode" then com = coms:Get(14)
			elseif k == "readyTip" then com = coms:Get(15)
			elseif k == "cancelButton" then com = coms:Get(16)
			elseif k == "startButton" then com = coms:Get(17)
			elseif k == "startNormal" then com = coms:Get(18)
			elseif k == "startForbit" then com = coms:Get(19)
			elseif k == "startTipNode" then com = coms:Get(20)
			elseif k == "startTip" then com = coms:Get(21)
			elseif k == "maskBg2" then com = coms:Get(22)
			elseif k == "gameTimeBegin" then com = coms:Get(23)
			elseif k == "txtGameTime" then com = coms:Get(24)
			elseif k == "typeNode" then com = coms:Get(25)
			elseif k == "typeDisplayText" then com = coms:Get(26)
			elseif k == "typeButton" then com = coms:Get(27)
			elseif k == "publishRuleNode" then com = coms:Get(28)
			elseif k == "publishBgBtn" then com = coms:Get(29)
			elseif k == "closePublishRule" then com = coms:Get(30)
			elseif k == "publishRuleTitle" then com = coms:Get(31)
			elseif k == "publishRuleDesc" then com = coms:Get(32) end
			rawset(t, k, com)
			return com
		end
	})
end

return BaseUIBilliardsReady