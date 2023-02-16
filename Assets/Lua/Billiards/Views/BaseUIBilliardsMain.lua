---@class BaseUIBilliardsMain : UIGameView
local BaseUIBilliardsMain = class("BaseUIBilliardsMain", UIGameView)

-- Properties
BaseUIBilliardsMain.static.prefabPath = "Game/Billiards/Views/UIBilliardsMain.prefab"

function BaseUIBilliardsMain:OnCreated()
	UIGameView.OnCreated(self)
	local coms = self.component:GetComponents(self.transform, true)
	self.view = setmetatable({}, {
		__index = function(t, k)
			local com = nil
			if k == "content" then com = coms:Get(0)
			elseif k == "mainPanel" then com = coms:Get(1)
			elseif k == "player_1" then com = coms:Get(2)
			elseif k == "playerHead_1" then com = coms:Get(3)
			elseif k == "btHead" then com = coms:Get(4)
			elseif k == "playerTimeImg_1" then com = coms:Get(5)
			elseif k == "playerBallBar_1" then com = coms:Get(6)
			elseif k == "playerBall_1" then com = coms:Get(7)
			elseif k == "player_2" then com = coms:Get(8)
			elseif k == "playerHead_2" then com = coms:Get(9)
			elseif k == "btHead1" then com = coms:Get(10)
			elseif k == "playerTimeImg_2" then com = coms:Get(11)
			elseif k == "playerBallBar_2" then com = coms:Get(12)
			elseif k == "playerBall_2" then com = coms:Get(13)
			elseif k == "sliderPower" then com = coms:Get(14)
			elseif k == "sliderPowerCue" then com = coms:Get(15)
			elseif k == "imgPowerCancel" then com = coms:Get(16)
			elseif k == "objAngleCtrl" then com = coms:Get(17)
			elseif k == "angleSlider" then com = coms:Get(18)
			elseif k == "spineAnim" then com = coms:Get(19)
			elseif k == "spineEnterBall_1" then com = coms:Get(20)
			elseif k == "spineEnterBall_2" then com = coms:Get(21)
			elseif k == "spineEnterBall_3" then com = coms:Get(22)
			elseif k == "spineEnterBall_4" then com = coms:Get(23)
			elseif k == "spineEnterBall_5" then com = coms:Get(24)
			elseif k == "spineEnterBall_6" then com = coms:Get(25)
			elseif k == "tablePos" then com = coms:Get(26) end
			rawset(t, k, com)
			return com
		end
	})
end

return BaseUIBilliardsMain