local UIBilliardsSelect = class("UIBilliardsSelect", BaseUIBilliardsSelect)
local BillardsModuleType = BillardsType.BillardsModuleType
local BillardsEvent = BillardsEvent.BillardsEventType
function UIBilliardsSelect:OnOpened()
    local gameType = BilliardsBattle.GetModeType() or BillardsModuleType.NORMAL

    self.view.onStateEasy:SetActive(gameType == BillardsModuleType.EASY)
    self.view.onStateNormal:SetActive(gameType == BillardsModuleType.NORMAL)
end

function UIBilliardsSelect:OnValueChanged(com, value)
   local gameType = nil
   if value then
        if com == self.view.togNormal then
           gameType = BillardsModuleType.NORMAL
        elseif com == self.view.togEasy then
           gameType = BillardsModuleType.EASY
        end
   end
   self:SendGameChange(gameType)
   self:Close()
end

function UIBilliardsSelect:OnButtonClick(com)
    if com == self.view.btnClose then
        self:Close()
    end
end

function UIBilliardsSelect:SendGameChange(gametype)
    -- 关闭弹窗时候再请求接口
    local config_type_data = 
        json.encode(
        {
            {
                field = BillardsType.FieldType,
                value = gametype
            }
        }
    )
    VoiceSender.ChangeGameType(config_type_data)
end

return UIBilliardsSelect