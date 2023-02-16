local UIBilliardsMenu = class("UIBilliardsMenu", BaseUIBilliardsMenu)
UIBilliardsMenu:include(UIMenuViewMixin)

function UIBilliardsMenu:OnOpened()
    self:OnMenuOpened()
end

function UIBilliardsMenu:OnButtonClick(com)
    if com == self.view.ruleBtn then
        self.root:OpenAsync(UIBilliardsRule)
    end
    self:OnMenuButtonClick(com)
end

return UIBilliardsMenu