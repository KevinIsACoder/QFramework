local UIBilliardsContainer = class("UIBilliardsContainer", BaseUIBilliardsContainer)
local EventDef = BillardsEvent.BillardsEventType

function UIBilliardsContainer:OnOpened()
    self.view.headContainer:Open(UICommonTopPage)
    self:RegisterEvent()
end

function UIBilliardsContainer:RegisterEvent()
    self.eventManager:Add(EventDef.E_BeginRound, function(_, data)
        self.view.popViewRoot:CloseAll(false)
        self.view.panelViewRoot:CloseAll(false)
        self.view.panelViewRoot:Open(UIBilliardsMain, UIContext.Get({content = self.view.content}))
    end)

    self.eventManager:Add(EventDef.E_EndRound, function()
        self.view.panelViewRoot:CloseAll(false)
        self.view.panelViewRoot:Open(UIBilliardsReady)
    end)

    self.eventManager:Add(EventDef.E_GameOver, function(_, data)
        self.view.popViewRoot:Open(UIBilliardsResult, UIContext.Get({resultdata = data.users}))
    end)
end

function UIBilliardsContainer:OnClosed()
    -- body
end

return UIBilliardsContainer